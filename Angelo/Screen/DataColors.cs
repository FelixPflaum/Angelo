using System;

namespace Angelo.Screen
{
    [Flags]
    public enum DataColors
    {
        InCombat = 0x000040,
        LureActive = 0x000080,
        Casting = 0x008000,
        TooltipShown = 0x800000,
    }
}
