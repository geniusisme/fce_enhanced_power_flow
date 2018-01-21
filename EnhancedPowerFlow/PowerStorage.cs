using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using static FortressCraft.ModFoundation.Util;

namespace GeniusIsme
{
public class PowerStorage : PowerConsumerInterface
{
    /// Storage recive power from other entities while their update and zeroes it while self update
    public float Recieved { get; private set; }
    /// Storage deliver power to other entities while self update
    public float Delivered { get; private set; }
    public float Power { get; set; }

    public readonly float Capacity;
    public readonly float OutputRate;
    public readonly float InputRate;

    int StartConsumerIndex;

    public PowerStorage(float capacity, float outputRate, float inputRate)
    {
        this.Capacity = capacity;
        this.OutputRate = ToPerTickRate(outputRate);
        this.InputRate = ToPerTickRate(inputRate);
    }

    public void Update(float delta, IEnumerable<PowerConsumerInterface> consumers)
    {
        this.Delivered = 0;
        this.Recieved = 0;
        var ticks = ToTicks(delta);
        var memoized = consumers.ToArray();
        if (memoized.Count() == 0)
        {
            return;
        }
        this.StartConsumerIndex = (this.StartConsumerIndex + 1) % memoized.Count();
        var fairConsumers = memoized.Skip(StartConsumerIndex).Concat(memoized.Take(StartConsumerIndex));
        foreach (var consumer in fairConsumers)
        {
            this.FeedConsumer(consumer, ticks);
        }
    }

    void FeedConsumer(PowerConsumerInterface consumer, float ticks)
    {
        var equalisedPower = EqualisedPower(consumer);
        if (this.Power > equalisedPower)
        {
            var transferAmount = new [] {
                this.OutputRate * ticks,
                this.Power - equalisedPower,
                consumer.GetRemainingPowerCapacity(),
                consumer.GetMaximumDeliveryRate() * ticks,
            }.Min();
            if (consumer.DeliverPower(transferAmount))
            {
                this.Power -= transferAmount;
                this.Delivered += transferAmount;
            }
        }
    }

    float EqualisedPower(PowerConsumerInterface them)
    {
        var sumCapacity = this.Capacity + them.GetMaxPower();
        var sumPower = this.Power + them.GetMaxPower() - them.GetRemainingPowerCapacity();
        return this.Capacity * sumPower / sumCapacity;
    }

    static float NormalizedPower(PowerConsumerInterface powerHolder)
    {
        return 1f - powerHolder.GetRemainingPowerCapacity() / powerHolder.GetMaxPower();
    }

    public float GetMaxPower()
    {
        return this.Capacity;
    }

    public float GetRemainingPowerCapacity()
    {
        return this.Capacity - this.Power;
    }

    public float GetMaximumDeliveryRate()
    {
        return this.InputRate;
    }

    public bool DeliverPower(float amount)
    {
        if (amount > this.GetRemainingPowerCapacity())
        {
            return false;
        }
        this.Power += amount;
        this.Recieved += amount;
        return true;
    }

    public bool WantsPowerFromEntity(SegmentEntity entity)
    {
        return true;
    }

    public void Read(BinaryReader reader)
    {
        if (this.Capacity < 1 << 8) this.Power = reader.ReadByte();
        else if (this.Capacity < 1 << 16) this.Power = reader.ReadUInt16();
        else this.Power = reader.ReadUInt32();
    }

    public void Write(BinaryWriter writer)
    {
        if (this.Capacity < 1 << 8) writer.Write((Byte)this.Power);
        else if (this.Capacity < 1 << 16) writer.Write((UInt16)this.Power);
        else writer.Write((UInt32)this.Power);
    }
}
}
