using System;
using System.Text.Json;
using System.IO;
using System.Windows;

namespace Angelo.Settings
{
    internal static class SettingsManager
    {
        private const string FILE_NAME = "Angelo_settings.json";

        private static readonly Object _lock = new();
        private static readonly SettingsData _settings = new();
        private static bool _initialized = false;
        private static bool _haveDataFromFile = false;

        public static SettingsData GetSettings()
        {
            if (!_initialized)
            {
                lock (_lock)
                {
                    if (!_initialized)
                        Load();
                }
            }
            return _settings;
        }

        /// <summary>
        /// Attempt to load settings data from file.
        /// </summary>
        public static void Load()
        {
            lock (_lock)
            {
                if (File.Exists(FILE_NAME))
                {
                    string cfgFile;
                    SettingsData? parsed;

                    try
                    {
                        cfgFile = File.ReadAllText(FILE_NAME);

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error while loading settings file!", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    try
                    {
                        parsed = JsonSerializer.Deserialize<SettingsData>(cfgFile);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error while parsing settings data!", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (parsed != null)
                    {
                        _settings.SetFromData(parsed);
                        _haveDataFromFile = true;
                    }
                }

                _initialized = true;
            }
        }

        /// <summary>
        /// Safe settings to file.
        /// </summary>
        public static void Safe()
        {
            JsonSerializerOptions options = new() { WriteIndented = true };

            try
            {
                string jsoned = JsonSerializer.Serialize(_settings, options);
                File.WriteAllText(FILE_NAME, jsoned);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error while saving settings!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Check if settings were loaded from file.
        /// </summary>
        /// <returns>True if settings were loaded.</returns>
        public static bool WereSettingsLoaded()
        {
            return _haveDataFromFile;
        }
    }
}
