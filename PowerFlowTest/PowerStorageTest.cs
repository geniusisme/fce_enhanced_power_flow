using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GeniusIsme;

using static FortressCraft.ModFoundation.Util;

namespace PowerFlowTest
{
[TestClass]
public class PowerStorageTest
{
    const float PPS = 150f;

    Storages Storages = new Storages();

    void Update(int sIdx)
    {
        var storage = Storages.PowerStorages[sIdx] as TestStorage;
        if (storage == null) return;
        var neighbours = new List<PowerConsumerInterface>();
        var connections = Storages.Connections[sIdx];
        foreach (int nIdx in connections)
        {
            neighbours.Add(Storages.PowerStorages[nIdx]);
        }
        Storages.PowerStorages[sIdx].Update(0.2f, neighbours);
    }

    void Update()
    {
        for (int sIdx = 0; sIdx < Storages.PowerStorages.Count(); ++sIdx)
        {
            Update(sIdx);
        }
    }

    float Power(int index)
    {
        return Storages.PowerStorages[index].GetMaxPower() - Storages.PowerStorages[index].GetRemainingPowerCapacity();
    }

    void AssertClose(float ideal, float real, int ticks = 1)
    {
        if (Math.Abs(ideal - real) > ticks * ToPerTickRate(PPS))
        {
            // this hack will give you the value
            Assert.AreEqual(ideal, real);
        }
    }

    [TestMethod]
    public void TakePowerButWontGiveBack()
    {
        Storages = new Storages
        {
            {newStorage(200, 100), 1},
            {newStorage(200)}
        };

        Update(0);
        var power0 = Power(0);
        Update(1);

        Assert.IsTrue(Power(0) < 100); //transferred
        Assert.AreEqual(power0, Power(0)); // didn't receive anything back
        Assert.IsTrue(Power(1) > 0); //received something
    }

    [TestMethod]
    public void TransferToBiggerBatteriesPossible()
    {
        Storages = new Storages
        {
            {newStorage(200, 100), 1},
            {newStorage(5000, 100)}
        };

        Update(0);
        Update(1);

        Assert.IsTrue(Power(0) < 100); //transferred
        Assert.IsTrue(Power(1) > 100); //received something
    }

    [TestMethod]
    public void PowerGetsTransferredEventually()
    {
        Storages = new Storages
        {
            {newStorage(1500, 1500)},
            {newStorage(200, 200 - ToPerTickRate(PPS)), 0, 2},
            {newStorage(1500, 1500)}
        };

        for (int i = 0; i < 10000; ++i)
        {
            Update();
            AssertClose(3200 - ToPerTickRate(PPS), Power(0) + Power(1) + Power(2), 2);
        }
    }

    [TestMethod]
    public void PowerGetsEvenEventually()
    {
        Storages = new Storages
        {
            {newStorage(1500, 0)},
            {newStorage(200, 100), 0, 2},
            {newStorage(5000, 1300)}
        };

        for (int i = 0; i < 100; ++i) Update();

        AssertClose(300, Power(0), 2);
        AssertClose(1000, Power(2), 2);
    }

    [TestMethod]
    public void FeedingOneFillsOther()
    {
        Storages = new Storages
        {
            {newStorage(5000), 1},
            {newStorage(5000)}
        };

        float delivered = 0f;
        for (int i = 0; i < 260; ++i)
        {
            var storage = Storages.PowerStorages[0];
            var toDeliver = new[]{ 100, storage.GetRemainingPowerCapacity() }.Min();
            storage.DeliverPower(toDeliver);
            delivered += toDeliver;
            Update();
        }
        Assert.AreEqual(10000, Power(0) + Power(1));
        AssertClose(Power(0) + Power(1), delivered);
    }

    [TestMethod]
    public void DeliverAllThePowerToRegularConsumer()
    {
        Storages = new Storages
        {
            {newStorage(100, 100), 1},
            {newConsumer(100)}
        };       

        for (int i = 0; i < 100 / ToPerTickRate(PPS); ++i)
        {
            Update();
        }
        AssertClose(Power(0), 0);
        AssertClose(Power(1), 100);
    }

    PowerStorage newStorage(float capacity, float power = 0)
    {
        var storage = new TestStorage(capacity, PPS, PPS);
        storage.DeliverPower(power);
        return storage;
    }

    PowerStorage newConsumer(float capacity, float power = 0)
    {
        var consumer = new PowerStorage(capacity, PPS, PPS);
        consumer.DeliverPower(power);
        return consumer;
    }
}

class TestStorage : PowerStorage, PowerStorageInterface
{
    public TestStorage(float capacity, float outputRate, float inputRate) :
        base(capacity, outputRate, inputRate) {}

    public float PowerDeltaPPS { get; set; }

    public float PowerDelta { get; private set; }

    public float PreviousPower { get; private set; }

    public int AttachedPowerConsumers { get; private set; }

    public float CurrentPower { get; set; }
}


class Storages : IEnumerable<int>
{
    public List<PowerStorage> PowerStorages = new List<PowerStorage>();
    public Dictionary<int, List<int>> Connections
        = new Dictionary<int, List<int>>();

    public IEnumerator<int> GetEnumerator() => null;

    IEnumerator IEnumerable.GetEnumerator() => null;

    public void Add(PowerStorage storage, params int[] connections)
    {
        PowerStorages.Add(storage);
        var idx = PowerStorages.Count() - 1;
        var from = At(idx);
        foreach (int connection in connections)
        {
            from.Add(connection);
            At(connection).Add(idx);
        }
    }

    List<int> At(int idx)
    {
        var result = new List<int>();
        if (Connections.ContainsKey(idx))
        {
            result = Connections[idx];
        }
        else
        {
            Connections.Add(idx, result);
        }
        return result;
    }
}
}
