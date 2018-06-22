using System.IO;
using System.Linq;
using FortressCraft.ModFoundation;
using FortressCraft.ModFoundation.Block;
using FortressCraft.ModFoundation.Multiblock;

namespace GeniusIsme
{
/// basic class for power storing machines, using enchanced power flow mechanic
public class PowerStorageMachine<G> : OverloadedMachine<G>, PowerConsumerInterface, PowerStorageInterface
    where G : MachineEntity
{
    protected PowerStorage Storage;
    protected WindowAverage In = new WindowAverage(1.0f);
    protected WindowAverage Out = new WindowAverage(1.0f);
    PciSurveyor Surveyor;

    /// override this method to provide underlying graphics with info it needs to update
    /// also useful for updating status text
    /// power - current power, recived and delivered to others in previous tick
    public virtual void Update(float power, float recieved, float delivered, float timeDelta) {}

    /// provide configured storage, graphics, and occupied box (single or mulit-block)
    public PowerStorageMachine(ModCreateSegmentEntityParameters parameters, PowerStorage storage, Position size, G graphics)
       : base(parameters, graphics)
    {
        Storage = storage;
        var center = new Position(parameters);
        var shift = size / 2;
        var box = new GridBox(new Box(center - shift, center + shift));
        var probes = Direction.All().SelectMany(
            (d) => box.Side(d).Select((p) => new PciSurveyor.Probe(p, d))
        );
        Surveyor = new PciSurveyor(probes, this);
    }

    public override void Update(float timeDelta)
    {
        UpdateImportantCPH();
        var surveyed = Surveyor.Survey();
        AttachedPowerConsumers = surveyed.Count();
        var transmission = Storage.Update(timeDelta, surveyed);
        PowerDelta = transmission.Recieved - transmission.Delivered;
        PreviousPower = Storage.Power - PowerDelta;
        In.AddMeasurement(transmission.Recieved / timeDelta, timeDelta);
        Out.AddMeasurement(transmission.Delivered / timeDelta, timeDelta);
        Update(Storage.Power, transmission.Recieved, transmission.Delivered, timeDelta);

    }

    private void UpdateImportantCPH()
    {
        if (!DifficultySettings.mbImportantCPH || !CentralPowerHub.Destroyed)
            return;
        Storage.Power *= 0.95f;
    }

    public float PowerDeltaPPS { get { return In.Value - Out.Value; } }

    public float PowerDelta { get; private set; }

    public float PreviousPower { get; private set; }

    public int AttachedPowerConsumers { get; private set; }

    public float CurrentPower { get { return Storage.Power; } set { Storage.Power = value; } }

    public float GetMaxPower()
    {
        return Storage.GetMaxPower();
    }

    public float GetRemainingPowerCapacity()
    {
        return Storage.GetRemainingPowerCapacity();
    }

    public float GetMaximumDeliveryRate()
    {
        return Storage.GetMaximumDeliveryRate();
    }

    public bool DeliverPower(float amount)
    {
        return Storage.DeliverPower(amount);
    }

    public bool WantsPowerFromEntity(SegmentEntity entity)
    {
        return Storage.WantsPowerFromEntity(entity);
    }

    public override void Write(BinaryWriter writer)
    {
        Storage.Write(writer);
    }

    public override void Read(BinaryReader reader, int entityVersion)
    {
        Storage.Read(reader);
    }

    public override int GetVersion()
    {
        return 1;
    }

    public override void WriteNetworkUpdate(BinaryWriter writer)
    {
        Storage.Write(writer);
    }

    public override void ReadNetworkUpdate(BinaryReader reader)
    {
        Storage.Read(reader);
    }
}
}
