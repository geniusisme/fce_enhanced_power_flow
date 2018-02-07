using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using FortressCraft.ModFoundation;
using FortressCraft.ModFoundation.Block;
using FortressCraft.ModFoundation.Multiblock;

namespace GeniusIsme
{
public class Conduit : OverloadedMachine<T4_Conduit>, IControl<Conduit>, PowerConsumerInterface
{
    /// orientation for model to transfer in ray direction
    public static Orientation OrientAlongRay(Direction ray)
    {
        var newX = ray.IsAlong(Direction.PlusY())? Direction.PlusX(): Direction.PlusY();
        var newY = newX.RotateCcw(ray.Negate());
        return new Orientation(newX, newY);
    }

    public Conduit(ModCreateSegmentEntityParameters parameters):
        base(parameters,
            new T4_Conduit(
                parameters.Segment,
                parameters.X,
                parameters.Y,
                parameters.Z,
                parameters.Cube,
                parameters.Flags,
                1,
                false
            )
        )
    {
        var orientation = Orientation.MakeFromFlags(parameters.Flags);
        this.TransferDir = orientation.ApplyTo(Direction.PlusZ());
        var center = new Position(parameters);
        var box = new Box(center, center);
        foreach(var dir in Direction.All())
        {
            if (!dir.IsAlong(this.TransferDir))
            {
                box = box.Extended(dir);
            }
        }
        this.Box = new GridBox(box);
        this.Surveyor = new BlockSurveyor(this);
        this.Connector = new Connector<Conduit>(
            this,
            EnhancedPowerFlow.Conduit,
            new GridBox(box).Blocks()
        );
        StartLookingForBattery();
    }

    public override void Update(float timeDelta)
    {
        switch(this.State)
        {
            case ConduitState.LookForBattery: this.LookForBattery(); break;
            case ConduitState.LookForOpposite: this.LookForOpposite(); break;
            case ConduitState.Connected: this.UpdateConnected(timeDelta); break;
        }
        this.StateTimer = this.StateTimer + timeDelta;
    }

    void LookForBattery()
    {
        var direction = this.TransferDir;
        var battery = this.BatteryAt(direction);
        if (battery == null)
        {
            direction = direction.Negate();
            battery = this.BatteryAt(direction);
        }
        if (battery != null)
        {
            this.ConsumerDelegate = battery;
            this.TransferDir = direction.Negate();
            this.Vanilla.OnUpdateRotation(OrientAlongRay(this.TransferDir).Flags());
            StartLookingForOpposite();
        }
    }

    PowerConsumerInterface BatteryAt(Direction direction)
    {
        var blocks = this.Box.Side(direction)
            .Select(block => this.Surveyor.Look().At(block).For<PowerStorageFiller<BatteryMk4>>());
        var center = blocks.First()?.Control;
        if (center != null && blocks.Skip(1).All(b => b.Control == center))
        {
            return (BatteryMk4)center;
        }
        else
        {
            return null;
        }
    }

    void LookForOpposite()
    {
        if (this.BatteryAt(this.TransferDir.Negate()) != this.ConsumerDelegate)
        {
            StartLookingForBattery();
            return;
        }
        var center = this.Box.Box.Center();
        var opposite = Enumerable.Range(this.Searched, 16)
            .Select(len => center + this.TransferDir.Shift * len)
            .Select(block => this.Surveyor.Look().At(block).For<Conduit>())
            .Index()
            .Where(opp => opp.Value != null)
            .Find(opp => opp.Value.State == ConduitState.LookForOpposite && opp.Value.TransferDir == this.TransferDir.Negate());

        if (opposite != null)
        {
            var distance = this.Searched + opposite.Value.Index;
            this.ConnectTo(opposite.Value.Value, distance);
            opposite.Value.Value.ConnectTo(this, distance);
        }
        this.Searched = (this.Searched + 16) % 128;
    }

    void UpdateConnected(float timeDelta)
    {
        if (this.BatteryAt(this.TransferDir.Negate()) != this.ConsumerDelegate)
        {
            this.StartLookingForBattery();
        } else {
            this.PowerTransfer.AddMeasurement(this.PowerTransferredLastTick / timeDelta, timeDelta);
            this.PowerTransferredLastTick = 0f;
        }
    }

    void StartLookingForBattery()
    {
        this.ConsumerDelegate = null;
        this.State = ConduitState.LookForBattery;
        this.Opposite?.StartLookingForOpposite();
        this.Vanilla.mState = T4_Conduit.eConduitState.eCantFindConduit;
    }

    void StartLookingForOpposite()
    {
        this.Opposite = null;
        this.State = ConduitState.LookForOpposite;
        this.Searched = 0;
        this.SetVanillaState(T4_Conduit.eConduitState.eCantFindConduit);
    }

    void ConnectTo(Conduit them, int distance)
    {
        this.State = ConduitState.Connected;
        this.Opposite = them;
        this.Vanilla.mrConduitDistance = distance;
        this.SetVanillaState(T4_Conduit.eConduitState.eLinkedToConduit);
    }

    void SetVanillaState(T4_Conduit.eConduitState state)
    {
        this.Vanilla.mState = state;
        this.StateTimer = 0.0f;
    }

    public override string GetPopupText()
    {
        switch (this.State)
        {
            case ConduitState.LookForBattery: return "Not connected to battery";
            case ConduitState.LookForOpposite: return "Looking for opposite conduit to connect to";
        }
        var transfer = this.PowerTransfer.Value >= 0?
            "sending " + Math.Round(this.PowerTransfer.Value, 0):
            "receiving " + Math.Round(-this.PowerTransfer.Value, 0);
        return "Conduit\n" + transfer + " pps";
    }

    public Connector<Conduit> Connector { get; set; }

    public override void LowFrequencyUpdate()
    {
        if (!this.Connector.Operational())
        {
            return;
        }

        base.LowFrequencyUpdate();
    }

    public float GetMaxPower()
    {
        return this.Opposite?.ConsumerDelegate?.GetMaxPower() ?? 0;
    }

    public float GetRemainingPowerCapacity()
    {
        return this.Opposite?.ConsumerDelegate?.GetRemainingPowerCapacity() ?? 0;
    }

    public float GetMaximumDeliveryRate()
    {
        return this.Opposite?.ConsumerDelegate?.GetMaximumDeliveryRate() ?? 0;
    }

    public bool DeliverPower(float amount)
    {
        if (this.Opposite != null)
        {
            if (amount > 512)
            {
                this.Vanilla.mrVisualTimeSinceTransmission = 0.0f;
                this.Vanilla.mrBeamTime = 0.0f;
                this.Opposite.Vanilla.mrBeamTime = 0.0f;
                this.Transmitted = true;
            }

            if (this.Opposite.ConsumerDelegate.DeliverPower(amount))
            {
                this.PowerTransferredLastTick += amount;
                this.Opposite.PowerTransferredLastTick -= amount;
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    public bool WantsPowerFromEntity(SegmentEntity entity)
    {
        return this.Opposite?.ConsumerDelegate?.WantsPowerFromEntity(entity) ?? false;
    }

    public override void OnDelete()
    {
        this.Opposite?.StartLookingForOpposite();
        base.OnDelete();
        this.Connector.OnDelete();
    }

    Direction TransferDir;
    ConduitState State;
    PowerConsumerInterface ConsumerDelegate;
    BlockSurveyor Surveyor;
    GridBox Box;
    Conduit Opposite;
    int Searched;
    WindowAverage PowerTransfer = new WindowAverage(1.0f);
    float PowerTransferredLastTick;

    static FieldInfo StateTimerField = typeof(T4_Conduit).GetField("mrStateTimer", BindingFlags.NonPublic | BindingFlags.Instance);
    float StateTimer
    {
        get { return (float) StateTimerField.GetValue(this.Vanilla); }
        set { StateTimerField.SetValue(this.Vanilla, value); }
    }
    static FieldInfo TransmittedField = typeof(T4_Conduit).GetField("mbLFActivatedBeamThisFrame", BindingFlags.NonPublic | BindingFlags.Instance);
    bool Transmitted
    {
        set { TransmittedField.SetValue(this.Vanilla, value); }
    }
}

enum ConduitState
{
    LookForBattery,
    LookForOpposite,
    Connected,
}

}
