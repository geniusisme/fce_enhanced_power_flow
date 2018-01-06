using System;
using FortressCraft.ModFoundation.Multiblock;

namespace GeniusIsme
{
public class PowerStorageFiller<Machine> : Filler<Machine>, PowerConsumerInterface
    where Machine : MachineEntity, IControl<Machine>
{
    public PowerStorageFiller(ModCreateSegmentEntityParameters parameters)
    : base(parameters)
    {}

    float PowerConsumerInterface.GetMaxPower()
    {
        return this.Consumer?.GetMaxPower() ?? 0;
    }

    float PowerConsumerInterface.GetRemainingPowerCapacity()
    {
        return this.Consumer?.GetRemainingPowerCapacity() ?? 0;
    }

    float PowerConsumerInterface.GetMaximumDeliveryRate()
    {
        return this.Consumer?.GetMaximumDeliveryRate() ?? 0;
    }

    bool PowerConsumerInterface.DeliverPower(float amount)
    {
        return this.Consumer?.DeliverPower(amount) ?? false;
    }

    bool PowerConsumerInterface.WantsPowerFromEntity(SegmentEntity entity)
    {
        return this.Consumer?.WantsPowerFromEntity(entity) ?? false;
    }

    PowerConsumerInterface Consumer
    {
        get
        {
            return this.Control as PowerConsumerInterface;
        }
    }
}
}
