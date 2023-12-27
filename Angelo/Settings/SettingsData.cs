namespace Angelo.Settings
{
    public class SettingsData
    {
        public int Sensitivity { get; set; } = 100;
        public int Threshold { get; set; } = 10;
        public bool UseLure { get; set; } = false;

        public SettingsData() { }

        public void SetFromData(SettingsData data)
        {
            Sensitivity = data.Sensitivity;
            Threshold = data.Threshold;
            UseLure = data.UseLure;
        }
    }
}
