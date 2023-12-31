using Angelo.Settings;
using static Angelo.KBM.KBMHelpers;

namespace Angelo.Bot
{
    internal class KeyboardHandler
    {
        private readonly SettingsData _settings;

        public KeyboardHandler()
        {
            _settings = SettingsManager.GetSettings();
        }

        /// <summary>
        /// Send fishing keybind to currently active window.
        /// </summary>
        /// <returns>True if keys were send successfully.</returns>
        public bool SendFish()
        {
            byte[] keys = _settings.FishingKey.GetVKeyArray();
            
            if (keys.Length == 0)
                return false;

            return SendKeyboardInput(keys);
        }

        /// <summary>
        /// Send lure keybind to currently active window.
        /// </summary>
        /// <returns>True if keys were send successfully.</returns>
        public bool SendLure()
        {
            byte[] keys = _settings.LureKey.GetVKeyArray();

            if (keys.Length == 0)
                return false;

            return SendKeyboardInput(keys);
        }
    }
}
