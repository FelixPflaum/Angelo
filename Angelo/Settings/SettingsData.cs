﻿using Angelo.Keybinds;
using System.Windows.Input;

namespace Angelo.Settings
{
    internal class SettingsData
    {
        public Setting<int> Sensitivity { get; } = new(100);
        public Setting<int> Threshold { get; } = new(200);
        public Setting<bool> UseLure { get; } = new(false);
        public Setting<KeyBind> FishingKey { get; } = new(new KeyBind((byte)KeyInterop.VirtualKeyFromKey(Key.D6), KBM.KeyboardModifiers.CTRL));
        public Setting<KeyBind> LureKey { get; } = new(new KeyBind((byte)KeyInterop.VirtualKeyFromKey(Key.D7), KBM.KeyboardModifiers.CTRL));
        public Setting<int> BobberHue { get; } = new(6);
        public Setting<int> BobberHueTolerance { get; } = new(10);
        public Setting<int> BobberPixels { get; } = new(150);
    }
}
