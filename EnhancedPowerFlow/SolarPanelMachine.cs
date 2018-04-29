using System.Collections.Generic;
using System.Linq;
using FortressCraft.ModFoundation;
using FortressCraft.ModFoundation.Block;
using FortressCraft.ModFoundation.Multiblock;
using static FortressCraft.ModFoundation.Util;

namespace GeniusIsme
{
public class SolarPanelMachine<G> : OverloadedMachine<G>
    where G : MachineEntity
{
    public SolarPanelMachine(
        ModCreateSegmentEntityParameters parameters,
        Position size,
        G graphics,
        float dayRate,
        float nightRate)
        : base(parameters, graphics)
    {
        this.Surveyor = new BlockSurveyor(this);
        var center = new Position(parameters);
        var shift = size / 2;
        this.Box = new GridBox(new Box(center - shift, center + shift));
        this.Clearances = this.Box.Side(Direction.Up()).Select(b => new Clearance(b)).ToList();
        var difficultyFactor = 1.0f;
        if (DifficultySettings.mbEasyPower)
          difficultyFactor = 2.5f;
        if (DifficultySettings.mbRushMode)
          difficultyFactor = 3.5f;
        this.DayRate = dayRate * difficultyFactor;
        this.NightRate = nightRate * difficultyFactor;
        var down = Direction.Up().Negate();
        var probes = this.Box.Side(down)
            .Select((pos) => new PciSurveyor.Probe(pos, down));
        this.PciSurveyor = new PciSurveyor(probes, this);
    }

    public override void Update(float timeDelta)
    {
        var clearance = this.CheckClearance();
        var power = this.GeneratePower(clearance, timeDelta);
        this.DistributePower(power, timeDelta);
    }

    public override bool ShouldSave()
    {
        return false;
    }

    float CheckClearance()
    {
        if (new Position(this).Y < MinHeight)
        {
            return 0f;
        }

        var clearNum = this.Clearances
            .Where(c => { c.Update(this.Surveyor); return c.Clear; })
            .Count();
        return (float)clearNum / (float)this.Clearances.Count();
    }

    float GeneratePower(float clearance, float timeDelta)
    {
        var angle = SurvivalWeatherManager.mrSunAngle;
        if (angle > 0)
        {
            this.Generation = this.DayRate * angle * clearance;
        }
        else
        {
            this.Generation = -this.NightRate * angle * clearance;
        }
        return this.Generation * timeDelta;
    }

    void DistributePower(float power, float timeDelta)
    {
        foreach(var consumer in this.PciSurveyor.Survey())
        {
            var amount = new [] {
                power,
                consumer.GetRemainingPowerCapacity(),
                consumer.GetMaximumDeliveryRate() * ToTicks(timeDelta),
            }.Min();
            if (consumer.DeliverPower(amount))
            {
                power -= amount;
            }
        }
    }

    public override string GetPopupText()
    {
        return "Collecting light at rate of " + (int) this.Generation + "pps" ;
    }

    BlockSurveyor Surveyor;
    PciSurveyor PciSurveyor;
    List<Clearance> Clearances;
    GridBox Box;
    float DayRate = 1f;
    float NightRate = 0.1f;
    float Generation = 0f;
    private static readonly long MinHeight = 4611686017890516928L;
}

class Clearance
{
    public Clearance(Position StartBlock)
    {
        this.StartBlock = StartBlock;
        this.LastCheckedPosition = 0;
        this.Clear = false;
    }

    public void Update(BlockSurveyor Surveyor)
    {
        var from = this.StartBlock + Direction.Up().Shift * this.LastCheckedPosition;
        var to = from + Direction.Up().Shift * CheckChunk;
        var clear = new GridBox(new Box(from, to)).Blocks()
            .Select(b => CubeHelper.IsCubeSolid(Surveyor.Look().At(b).Block()?.Type ?? 1))
            .NoneTrue();

        if (clear)
        {
            this.LastCheckedPosition = (this.LastCheckedPosition + CheckChunk) % ClearHeight;
            if (this.LastCheckedPosition == 0)
            {
                this.Clear = true;
            }
        }
        else
        {
            this.Clear = false;
            this.LastCheckedPosition = 0;
        }
    }

    Position StartBlock;
    int LastCheckedPosition;
    public bool Clear { get; private set; }

    static readonly int CheckChunk = 8;
    static readonly int ClearHeight = 128;
}
}