using System;
using System.Text.Json;
using System.IO;
using System.Windows;

namespace Angelo.Settings
{
    internal class SettingsManager
    {
        private static readonly string FileName = "Angelo_settings.json";
        private static bool fileLoaded = false;

        public static readonly SettingsData settings = new();

        /// <summary>
        /// Attempt to load settings dada from file.
        /// </summary>
        public static void Load()
        {
            if (File.Exists(FileName))
            {
                string cfgFile;
                SettingsData? parsed;

                try
                {
                    cfgFile = File.ReadAllText(FileName);

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
                    settings.SetFromData(parsed);
                    fileLoaded = true;
                }
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
                string jsoned = JsonSerializer.Serialize(settings, options);
                File.WriteAllText(FileName, jsoned);
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
            return fileLoaded;
        }
    }
}
