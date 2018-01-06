namespace GeniusIsme
{
public class DummyPowerConsumer : PowerConsumerInterface
{
    public float GetMaxPower()
    {
        return 0;
    }

    public float GetRemainingPowerCapacity()
    {
        return 0;
    }

    public float GetMaximumDeliveryRate()
    {
        return 0;
    }

    public bool DeliverPower(float amount)
    {
        return false;
    }

    public bool WantsPowerFromEntity(SegmentEntity entity)
    {
        return false;
    }
}
}
