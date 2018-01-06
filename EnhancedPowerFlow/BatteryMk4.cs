using System.IO;
using FortressCraft.ModFoundation;
using FortressCraft.ModFoundation.Block;

namespace GeniusIsme
{
public class BatteryMk4 : PowerStorageControlBlock<global::T4_Battery, BatteryMk4>
{
    public BatteryMk4(ModCreateSegmentEntityParameters parameters):
        base(parameters, new PowerStorage(270000, 2000, 2000), new Position(3, 3, 3), EnhancedPowerFlow.BatteryMk4,
            new global::T4_Battery(
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
    }

    public override string GetPopupText()
    {
        return "MK4 Power Storage\n" +
               "Power : " + (int) this.Vanilla.mrCurrentPower + "/" + (int) this.Storage.Capacity + "\n";
    }
}
}
