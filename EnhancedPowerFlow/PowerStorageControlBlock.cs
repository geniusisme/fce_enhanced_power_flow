using FortressCraft.ModFoundation.Block;
using FortressCraft.ModFoundation.Multiblock;

namespace GeniusIsme
{
public class PowerStorageControlBlock<G, Machine> : PowerStorageMachine<G>, IControl<Machine>
    where Machine: MachineEntity, IControl<Machine>
    where G: MachineEntity
{
    public Connector<Machine> Connector { get; set; }

    public PowerStorageControlBlock(
        ModCreateSegmentEntityParameters parameters,
        PowerStorage storage,
        Position size,
        Materials materials,
        G graphics)
        : base(parameters, storage, size, graphics)
    {
        var center = new Position(parameters);
        var shift = size / 2;
        var box = new Box(center - shift, center + shift);

        this.Connector = new Connector<Machine>(
            this as Machine,
            materials,
            new GridBox(box).Blocks()
        );
    }

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