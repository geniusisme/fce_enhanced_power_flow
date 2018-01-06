using FortressCraft.ModFoundation.Block;
using FortressCraft.ModFoundation.Multiblock;

namespace GeniusIsme
{
public class OrganicSolarMk2 : SolarPanelMachine<Solar_MK2_Organic>, IControl<OrganicSolarMk2>
{
    public OrganicSolarMk2(ModCreateSegmentEntityParameters parameters)
        : base(
            parameters,
            new Position(3, 1, 3),
            new Solar_MK2_Organic(
                parameters.Segment,
                parameters.X,
                parameters.Y,
                parameters.Z,
                parameters.Cube,
                parameters.Flags,
                1,
                false),
            216f,
            21.6f)
    {
        var size = new Position(3, 1, 3);
        var center = new Position(parameters);
        var shift = size / 2;
        var box = new Box(center - shift, center + shift);
        this.Connector = new Connector<OrganicSolarMk2>(
            this,
            EnhancedPowerFlow.OrganicSolarMk2,
            new GridBox(box).Blocks()
        );
    }

    public Connector<OrganicSolarMk2> Connector { get; set; }

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