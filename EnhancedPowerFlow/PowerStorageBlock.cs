using System;
using UnityEngine;
using FortressCraft.ModFoundation;
using FortressCraft.ModFoundation.Block;

namespace GeniusIsme
{
public class PowerStorageBlock : PowerStorageMachine<global::PowerStorageBlock>
{
    WindowAverage PowerDelta = new WindowAverage(1.0f);

    public PowerStorageBlock(ModCreateSegmentEntityParameters parameters):
        base(
            parameters,
            MakeStorage(parameters.Value),
            new Position(1, 1, 1),
            new global::PowerStorageBlock(
                parameters.Segment,
                parameters.X,
                parameters.Y,
                parameters.Z,
                parameters.Cube,
                parameters.Flags,
                parameters.Value
            )
        )
    {
    }

    public override void Update(float power, float recieved, float delivered, float timeDelta)
    {
        this.PowerDelta.AddMeasurement((recieved - delivered) / timeDelta, timeDelta);
        this.Vanilla.mrNormalisedPower = power / this.GetMaxPower();
        this.Vanilla.mrCurrentPower = power;
        this.Vanilla.mrPowerSpareCapacity = this.Storage.Capacity - power;
    }

    public override string GetPopupText()
    {
        bool extract = Input.GetButton("Extract");
        bool store = Input.GetButton("Interact");

        var hadPower = this.Vanilla.mrCurrentPower;

        if (extract && !store)
        {
            PowerStorageBlockWindow.HoldExtract(this.Vanilla);
            this.RequestImmediateNetworkUpdate();
        }

        if (store && !extract)
        {
            PowerStorageBlockWindow.HoldInteract(this.Vanilla);
            this.RequestImmediateNetworkUpdate();
        }

        var delta = this.Vanilla.mrCurrentPower - hadPower;
        this.Storage.Power += delta;

        return "Power Storage\n" +
               "Power : " + Math.Round(this.Storage.Power, 0) + "/" + this.Storage.Capacity + "\n" +
               "Power delta : " + Math.Round(this.PowerDelta.Value, 1) + " pps\n" +
               "Press 'E' to to add power" + "\n" +
               "Press 'Q' to remove power";
    }

    static PowerStorage MakeStorage(int variety)
    {
        switch (variety)
        {
            case 0: return new PowerStorage(200, 75, 75);
            case 1: return new PowerStorage(1500, 400, 400);
            case 2: return new PowerStorage(5000, 2000, 2000);
            case 3: return new PowerStorage(8500, 100, 100);
        }
        return null;
    }
}
}
