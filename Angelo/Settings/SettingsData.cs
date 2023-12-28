using System.Collections.Generic;

namespace Angelo.Settings
{
    internal class SettingsData
    {
        public int Sensitivity { get; set; } = 100;
        public int Threshold { get; set; } = 10;
        public bool UseLure { get; set; } = false;
        public Dictionary<string, uint> KeyBinds { get; set; } = new();

        public SettingsData() { }

        public void SetFromData(SettingsData data)
        {
            Sensitivity = data.Sensitivity;
            Threshold = data.Threshold;
            UseLure = data.UseLure;
            KeyBinds = data.KeyBinds;
        }
    }
}
