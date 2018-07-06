using System;
using FortressCraft.ModFoundation.Multiblock;

namespace GeniusIsme
{
public class PowerStorageFiller<Machine> : Filler<Machine>, PowerConsumerInterface, PowerStorageInterface
    where Machine : MachineEntity, IControl<Machine>
{
    public PowerStorageFiller(ModCreateSegmentEntityParameters parameters)
    : base(parameters)
    {}

    public float PowerDeltaPPS { get { return Storage?.PowerDeltaPPS ?? 0; } }

    public float PowerDelta { get { return Storage?.PowerDelta ?? 0; } }

    public float PreviousPower { get { return Storage?.PreviousPower ?? 0; } }

    public int AttachedPowerConsumers { get { return Storage?.AttachedPowerConsumers?? 0; } }

    public float CurrentPower { get { return Storage?.CurrentPower ?? 0; }
                                set { if (Storage != null) Storage.CurrentPower = value; } }

    public float GetMaxPower()
    {
        return Consumer?.GetMaxPower() ?? 0;
    }

    public float GetRemainingPowerCapacity()
    {
        return Consumer?.GetRemainingPowerCapacity() ?? 0;
    }

    public float GetMaximumDeliveryRate()
    {
        return Consumer?.GetMaximumDeliveryRate() ?? 0;
    }

    public bool DeliverPower(float amount)
    {
        return Consumer?.DeliverPower(amount) ?? false;
    }

    public bool WantsPowerFromEntity(SegmentEntity entity)
    {
        return Consumer?.WantsPowerFromEntity(entity) ?? false;
    }

    PowerConsumerInterface Consumer
    {
        get
        {
            return Control as PowerConsumerInterface;
        }
    }

    PowerStorageInterface Storage
    {
        get
        {
            return Control as PowerStorageInterface;
        }
    }
}
}
