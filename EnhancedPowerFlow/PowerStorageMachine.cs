using System.IO;
using System.Linq;
using FortressCraft.ModFoundation;
using FortressCraft.ModFoundation.Block;
using FortressCraft.ModFoundation.Multiblock;

namespace GeniusIsme
{
/// basic class for power storing machines, using enchanced power flow mechanic
public class PowerStorageMachine<G> : OverloadedMachine<G>, PowerConsumerInterface
    where G : MachineEntity
{
    protected PowerStorage Storage;
    PciSurveyor Surveyor;

    /// override this method to provide underlying graphics with info it needs to update
    /// also useful for updating status text
    /// power - current power, recived and delivered to others in previous tick
    public virtual void Update(float power, float recieved, float delivered, float timeDelta) {}

    /// provide configured storage, graphics, and occupied box (single or mulit-block)
    public PowerStorageMachine(ModCreateSegmentEntityParameters parameters, PowerStorage storage, Position size, G graphics)
       : base(parameters, graphics)
    {
        this.Storage = storage;
        var center = new Position(parameters);
        var shift = size / 2;
        var box = new GridBox(new Box(center - shift, center + shift));
        var probes = Direction.All().SelectMany(
            (d) => box.Side(d).Select((p) => new PciSurveyor.Probe(p, d))
        );
        this.Surveyor = new PciSurveyor(probes, this);
    }

    public override void Update(float timeDelta)
    {
        var recieved = this.Storage.Recieved;
        this.Storage.Update(timeDelta, Surveyor.Survey());
        this.UpdateImportantCPH();
        this.Update(this.Storage.Power, recieved, this.Storage.Delivered, timeDelta);
    }

    private void UpdateImportantCPH()
    {
        if (!DifficultySettings.mbImportantCPH || !CentralPowerHub.Destroyed)
            return;
        this.Storage.Power *= 0.95f;
    }

    public float GetMaxPower()
    {
        return this.Storage.GetMaxPower();
    }

    public float GetRemainingPowerCapacity()
    {
        return this.Storage.GetRemainingPowerCapacity();
    }

    public float GetMaximumDeliveryRate()
    {
        return this.Storage.GetMaximumDeliveryRate();
    }

    public bool DeliverPower(float amount)
    {
        return this.Storage.DeliverPower(amount);
    }

    public bool WantsPowerFromEntity(SegmentEntity entity)
    {
        return this.Storage.WantsPowerFromEntity(entity);
    }

    public override void Write(BinaryWriter writer)
    {
        this.Storage.Write(writer);
    }

    public override void Read(BinaryReader reader, int entityVersion)
    {
        this.Storage.Read(reader);
    }

    public override int GetVersion()
    {
        return 1;
    }

    public override void WriteNetworkUpdate(BinaryWriter writer)
    {
        this.Storage.Write(writer);
    }

    public override void ReadNetworkUpdate(BinaryReader reader)
    {
        this.Storage.Read(reader);
    }
}
}
