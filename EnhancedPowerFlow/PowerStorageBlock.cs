using System;
using UnityEngine;
using FortressCraft.ModFoundation;
using FortressCraft.ModFoundation.Block;

namespace GeniusIsme
{
public class PowerStorageBlock : PowerStorageMachine<global::PowerStorageBlock>
{
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
        Vanilla.mrNormalisedPower = power / GetMaxPower();
        Vanilla.mrCurrentPower = power;
        Vanilla.mrPowerSpareCapacity = Storage.Capacity - power;
    }

    public override string GetPopupText()
    {
        bool extract = Input.GetButton("Extract");
        bool store = Input.GetButton("Interact");

        var hadPower = Vanilla.mrCurrentPower;

        if (extract && !store)
        {
            PowerStorageBlockWindow.HoldExtract(Vanilla);
            RequestImmediateNetworkUpdate();
        }

        if (store && !extract)
        {
            PowerStorageBlockWindow.HoldInteract(Vanilla);
            RequestImmediateNetworkUpdate();
        }

        var delta = Vanilla.mrCurrentPower - hadPower;
        Storage.Power += delta;

        return "Power Storage\n" +
               "Power : " + Math.Round(Storage.Power, 0) + "/" + Storage.Capacity + "\n" +
               "Power delta : " + Math.Round(PowerDeltaPPS, 1) + " pps\n" +
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
