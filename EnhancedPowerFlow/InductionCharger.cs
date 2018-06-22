using System;
using System.IO;
using FortressCraft.ModFoundation;
using FortressCraft.ModFoundation.Block;
using FortressCraft.ModFoundation.Multiblock;

namespace GeniusIsme
{
public class InductionCharger : PowerStorageControlBlock<global::InductionCharger, InductionCharger>
{
    float TotalDelivered;

    public InductionCharger(ModCreateSegmentEntityParameters parameters):
        base(parameters, new PowerStorage(1500, 1250, 6000), new Position(5, 1, 5), EnhancedPowerFlow.InductionCharger,
            new global::InductionCharger(
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
    }

    public override void Update(float power, float recieved, float delivered, float timeDelta)
    {
        TotalDelivered += delivered;
        Vanilla.mrCurrentPower = power;
    }

    public override string GetPopupText()
    {
        return "Induction Charger\n" +
               "Power : " + Math.Round(Vanilla.mrCurrentPower, 0) + "\n" +
               "Delivering : " + Math.Round(Out.Value, 1) + " pps\n" + "\n" +
               "Total power delivered : " + Math.Round(TotalDelivered, 0);
    }
}
}
