using Angelo.Keybinds;
using System.Text.Json.Serialization;

namespace Angelo.Settings
{
    internal class SettingsData
    {
        public int Sensitivity { get; set; } = 100;
        public int Threshold { get; set; } = 10;
        public bool UseLure { get; set; } = false;

        [JsonConverter(typeof(KeyBindJsonConverter))]
        public KeyBind FishingKey { get; set; } = new();

        [JsonConverter(typeof(KeyBindJsonConverter))]
        public KeyBind LureKey { get; set; } = new();

        public SettingsData() { }

        public void SetFromData(SettingsData data)
        {
            Sensitivity = data.Sensitivity;
            Threshold = data.Threshold;
            UseLure = data.UseLure;
            FishingKey = data.FishingKey;
            LureKey = data.LureKey;
        }
    }
}
