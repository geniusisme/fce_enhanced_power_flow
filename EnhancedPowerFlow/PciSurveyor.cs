using System.Collections.Generic;
using System.Linq;
using FortressCraft.ModFoundation;
using FortressCraft.ModFoundation.Block;

using static FortressCraft.ModFoundation.EnumeratorUtil;

namespace GeniusIsme
{
public class PciSurveyor
{
    IEnumerator<IndexedValue<Probe>> Probes;
    BlockSurveyor Surveyor;
    List<PowerConsumerInterface> Pcis;
    MachineEntity PowerSource;
    int StartConsumerIndex;

    public PciSurveyor(IEnumerable<Probe> probes, MachineEntity powerSource)
    {
        this.Probes = probes.Index().Loop().GetEnumerator();
        this.Surveyor = new BlockSurveyor(powerSource);
        this.Pcis = Enumerable.Repeat<PowerConsumerInterface>(null, probes.Count()).ToList();
        this.PowerSource = powerSource;
    }

    static bool IsGood(PowerConsumerInterface pci)
    {
        var entity = (SegmentEntity) pci;
        return entity != null && !entity.mbDelete;
    }

    public IEnumerable<PowerConsumerInterface> Survey()
    {
        this.Update();
        var memoized = this.Pcis
            .Where((x) => IsGood(x))
            .Where((x) => x.WantsPowerFromEntity(this.PowerSource));
        if (memoized.Count() == 0)
        {
            return memoized;
        }
        this.StartConsumerIndex = (this.StartConsumerIndex + 1) % memoized.Count();
        return memoized.Skip(this.StartConsumerIndex).Concat(memoized.Take(this.StartConsumerIndex));
    }

    public struct Probe
    {
        public Probe(Position pos, Direction dir)
        {
            this.Pos = pos;
            this.Dir = dir;
        }

        public readonly Position Pos;
        public readonly Direction Dir;
    }

    void Update()
    {
        this.Probes.MoveNext();
        var iter = this.Probes.Current;
        var savedPci = Pcis[iter.Index];
        if (!IsGood(savedPci))
        {
            var search = this.Surveyor
                .Look()
                .At(iter.Value.Pos);
            var material = search.Block();
            if (material == null)
            {
                this.Pcis[iter.Index] = null;
            }
            else if (material?.Type == eCubeTypes.EnergyGrommet)
            {
                this.Pcis[iter.Index] = this.Surveyor
                    .Look()
                    .At(iter.Value.Pos + iter.Value.Dir.Shift)
                    .For<PowerConsumerInterface>();
            }
            else
            {
                this.Pcis[iter.Index] = search.For<PowerConsumerInterface>();
            }
        }
    }
}
}
