using System.Collections.Generic;
using System.Linq;
using FortressCraft.ModFoundation.Block;
using FortressCraft.ModFoundation.Multiblock;

namespace GeniusIsme
{
public class ConduitBuilder : Builder
{
    public bool BuildIfPossible(ModCheckForCompletedMachineParameters parameters)
    {
        var materials = EnhancedPowerFlow.Conduit;
        var rays = new List<Direction> {
            Direction.PlusX(),
            Direction.PlusY(),
            Direction.PlusZ(),
        };
        return rays
            .Select(r => new BoxBuilder(this.OrientedBox(r), materials, Conduit.OrientAlongRay(r)))
            .Where(b => b.BuildIfPossible(parameters) == true)
            .Any();
    }

    Position OrientedBox(Direction ray)
    {
        return new Position(ray.Shift.Select(c => c == 0? 3L : 1L));
    }
}
}
