using System;

namespace Angelo.KBM
{
    /// <summary>
    /// Flags representing modifier keys. Starting at 3rd byte.
    /// </summary>
    [Flags]
    internal enum KeyboardModifiers
    {
        NONE = 0,
        SHIFT = 0x010000,
        CTRL = 0x020000,
        ALT = 0x040000,
        ALL = SHIFT + CTRL + ALT,
    }
}
