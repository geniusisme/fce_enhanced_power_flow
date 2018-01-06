using System.IO;
using FortressCraft.ModFoundation;
using FortressCraft.ModFoundation.Block;

namespace GeniusIsme
{
public class BatteryMk5 : PowerStorageControlBlock<global::T5_Battery, BatteryMk5>
{
    WindowAverage In = new WindowAverage(1.0f);
    WindowAverage Out = new WindowAverage(1.0f);

    public BatteryMk5(ModCreateSegmentEntityParameters parameters):
        base(parameters, new PowerStorage(4500000, 10000, 10000), new Position(5, 9, 5), EnhancedPowerFlow.BatteryMk5,
            new global::T5_Battery(
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
        this.Vanilla.mrCurrentPower = power;
        this.In.AddMeasurement(recieved, timeDelta);
        this.Out.AddMeasurement(delivered, timeDelta);
    }

    public override string GetPopupText()
    {
        return "MK5 Power Storage\n" +
               "Power : " + (int) this.Vanilla.mrCurrentPower + "/" + (int) this.Storage.Capacity + "\n" +
               "Power In :" + (int) this.In.Value + "\n" +
               "Power Out : " + (int) this.Out.Value + "\n";
    }
}
}
