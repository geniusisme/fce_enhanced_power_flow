using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GeniusIsme;
using FortressCraft.ModFoundation;
using FortressCraft.ModFoundation.Block;
using FortressCraft.ModFoundation.Multiblock;

using static FortressCraft.ModFoundation.Util;

namespace PowerFlowTest
{
[TestClass]
public class PowerStorageTest
{
    const float PPS = 150f;

    List<PowerStorage> storages;
    List<PowerConsumerInterface> consumers;

    void CreateStorages(params float[] capacities)
    {
        storages = new List<PowerStorage>();
        consumers = new List<PowerConsumerInterface>();

        foreach( var capacity in capacities)
        {
            var storage = new PowerStorage(capacity, PPS, PPS);
            this.storages.Add(storage);
            this.consumers.Add(storage);
        }
    }

    void Update(int index)
    {
        var neighbours = new List<PowerConsumerInterface>();
        if (index > 0) neighbours.Add(this.consumers[index - 1]);
        if (index < this.storages.Count() - 1) neighbours.Add(this.consumers[index + 1]);
        this.storages[index].Update(0.2f, neighbours);
    }

    void Update()
    {
        for (int i = 0; i < this.storages.Count(); ++i)
        {
            this.Update(i);
        }
    }

    float Power(int index)
    {
        return this.consumers[index].GetMaxPower() - this.consumers[index].GetRemainingPowerCapacity();
    }

    void DeliverPower(params float[] powers)
    {
        Assert.AreEqual(powers.Count(), this.storages.Count());
        for (int index = 0; index < powers.Count(); ++index)
        {
            this.consumers[index].DeliverPower(powers[index]);
            this.storages[index].Update(0, this.consumers);
        }
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
        this.CreateStorages(200, 200);
        this.DeliverPower(100, 0);

        this.Update(0);
        var power0 = this.Power(0);
        this.Update(1);

        Assert.IsTrue(this.Power(0) < 100); //transferred
        Assert.AreEqual(power0, this.Power(0)); // didn't receive anything back
        Assert.IsTrue(this.Power(1) > 0); //received something
    }

    [TestMethod]
    public void TransferToBiggerBatteriesPossible()
    {
        this.CreateStorages(200, 5000);
        this.DeliverPower(100, 100);

        this.Update(0);
        this.Update(1);

        Assert.IsTrue(this.Power(0) < 100); //transferred
        Assert.IsTrue(this.Power(1) > 100); //received something
    }

    [TestMethod]
    public void PowerGetsTransferredEventually()
    {
        this.CreateStorages(1500, 200, 1500);
        this.DeliverPower(1500, 200 - ToPerTickRate(PPS), 1500);

        for (int i = 0; i < 10000; ++i)
        {
            this.Update();
            AssertClose(3200 - ToPerTickRate(PPS), Power(0) + Power(1) + Power(2), 2);
        }
    }

    [TestMethod]
    public void PowerGetsEvenEventually()
    {
        this.CreateStorages(1500, 200, 5000);
        this.DeliverPower(0, 100, 1300);

        for (int i = 0; i < 100; ++i) this.Update();

        AssertClose(300, Power(0), 2);
        AssertClose(1000, Power(2), 2);
    }

    [TestMethod]
    public void FeedingOneFillsOther()
    {
        this.CreateStorages(5000, 5000);
        float delivered = 0f;
        for (int i = 0; i < 260; ++i)
        {
            var toDeliver = new[]{ 100, this.consumers[0].GetRemainingPowerCapacity() }.Min();
            this.consumers[0].DeliverPower(toDeliver);
            delivered += toDeliver;
            this.Update();
        }
        Assert.AreEqual(10000, Power(0) + Power(1));
        AssertClose(Power(0) + Power(1), delivered);
    }
}
}
