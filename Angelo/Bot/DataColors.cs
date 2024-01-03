using System;

namespace Angelo.Bot
{
    [Flags]
    public enum DataColors : uint
    {
        LureActive = 0x000000FF,
        Casting = 0x0000FF00,
        TooltipShown = 0x00FF0000,
        InCombat = 0x010000FF,
        Alive = 0x0100FF00,
    }
}
