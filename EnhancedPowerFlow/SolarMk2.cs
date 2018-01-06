using FortressCraft.ModFoundation.Block;
using FortressCraft.ModFoundation.Multiblock;

namespace GeniusIsme
{
public class SolarMk2 : SolarPanelMachine<Solar_MK2>, IControl<SolarMk2>
{
    public SolarMk2(ModCreateSegmentEntityParameters parameters)
        : base(
            parameters,
            new Position(3, 1, 3),
            new Solar_MK2(
                parameters.Segment,
                parameters.X,
                parameters.Y,
                parameters.Z,
                parameters.Cube,
                parameters.Flags,
                1,
                false),
            162f,
            0f)
    {
        var size = new Position(3, 1, 3);
        var center = new Position(parameters);
        var shift = size / 2;
        var box = new Box(center - shift, center + shift);
        this.Connector = new Connector<SolarMk2>(
            this,
            EnhancedPowerFlow.SolarMk2,
            new GridBox(box).Blocks()
        );
    }

    public Connector<SolarMk2> Connector { get; set; }

    public override void LowFrequencyUpdate()
    {
        if (!this.Connector.Operational())
        {
            return;
        }

        base.LowFrequencyUpdate();
    }

    public override void OnDelete()
    {
        base.OnDelete();
        this.Connector.OnDelete();
    }
}
}