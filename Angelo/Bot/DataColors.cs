using System;

namespace Angelo.Bot
{
    [Flags]
    public enum DataColors
    {
        InCombat = 0x000040,
        LureActive = 0x000080,
        Alive = 0x004000,
        Casting = 0x008000,
        TooltipShown = 0x800000,
    }
}
