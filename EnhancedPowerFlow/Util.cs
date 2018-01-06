using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FortressCraft.ModFoundation
{
public static class Util
{
    static float TICK_DURATION = 0.2f;
    static float TICKS_PER_SECOND = 1.0f / TICK_DURATION;

    static public float ToPerTickRate(float perSecond)
    {
        return perSecond * TICK_DURATION;
    }

    static public float ToPerSecondRate(float perTick)
    {
        return perTick * TICKS_PER_SECOND;
    }

    static public float ToTicks(float seconds)
    {
        return seconds * TICKS_PER_SECOND;
    }

    static public float ToSeconds(float ticks)
    {
        return ticks * TICK_DURATION;
    }
}


}
