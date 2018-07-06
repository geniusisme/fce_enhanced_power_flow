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
    float Recieved;
    float Delivered;
    float PowerField;
    public float Power
    {
        get { return PowerField; }
        set
        {
            var delta = value - PowerField;
            if (delta > 0)
            {
                Recieved += delta;
            }
            else
            {
                Delivered -= delta;
            }

            PowerField = value;
        }
    }

    public readonly float Capacity;
    public readonly float OutputRate;
    public readonly float InputRate;

    public PowerStorage(float capacity, float outputRate, float inputRate)
    {
        Capacity = capacity;
        OutputRate = ToPerTickRate(outputRate);
        InputRate = ToPerTickRate(inputRate);
    }

    public Transmission Update(float delta, IEnumerable<PowerConsumerInterface> consumers)
    {
        var ticks = ToTicks(delta);

        foreach (var consumer in consumers)
        {
            FeedConsumer(consumer, ticks);
        }

        var result = new Transmission(Recieved, Delivered);

        Delivered = 0;
        Recieved = 0;

        return result;
    }

    void FeedConsumer(PowerConsumerInterface consumer, float ticks)
    {
        var powerToHoldOn = 0f;
        if (consumer as PowerStorageInterface != null)
        {
            powerToHoldOn = EqualisedPower(consumer);
        }
        if (Power > powerToHoldOn)
        {
            var transferAmount = new [] {
                OutputRate * ticks,
                Power - powerToHoldOn,
                consumer.GetRemainingPowerCapacity(),
                consumer.GetMaximumDeliveryRate() * ticks,
            }.Min();
            if (consumer.DeliverPower(transferAmount))
            {
                Power -= transferAmount;
            }
        }
    }

    float EqualisedPower(PowerConsumerInterface them)
    {
        var sumCapacity = Capacity + them.GetMaxPower();
        var sumPower = Power + them.GetMaxPower() - them.GetRemainingPowerCapacity();
        return Capacity * sumPower / sumCapacity;
    }

    static float NormalizedPower(PowerConsumerInterface powerHolder)
    {
        return 1f - powerHolder.GetRemainingPowerCapacity() / powerHolder.GetMaxPower();
    }

    public float GetMaxPower()
    {
        return Capacity;
    }

    public float GetRemainingPowerCapacity()
    {
        return Capacity - Power;
    }

    public float GetMaximumDeliveryRate()
    {
        return InputRate;
    }

    public bool DeliverPower(float amount)
    {
        if (amount > GetRemainingPowerCapacity())
        {
            return false;
        }
        Power += amount;
        return true;
    }

    public bool WantsPowerFromEntity(SegmentEntity entity)
    {
        return true;
    }

    public void Read(BinaryReader reader)
    {
        if (Capacity < 1 << 8) Power = reader.ReadByte();
        else if (Capacity < 1 << 16) Power = reader.ReadUInt16();
        else Power = reader.ReadUInt32();
    }

    public void Write(BinaryWriter writer)
    {
        if (Capacity < 1 << 8) writer.Write((Byte)Power);
        else if (Capacity < 1 << 16) writer.Write((UInt16)Power);
        else writer.Write((UInt32)Power);
    }
}

public struct Transmission
{
    public readonly float Recieved;
    public readonly float Delivered;

    public Transmission(float recived, float delivered)
    {
        Recieved = recived;
        Delivered = delivered;
    }
}
}
