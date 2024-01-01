using System;
using System.IO;
using System.Windows;
using System.Reflection;
using Angelo.Keybinds;
using System.Text;

namespace Angelo.Settings
{
    internal static class SettingsManager
    {
        private const string FILE_NAME = "Angelo_settings";

        private static readonly SettingsData _settings = new();
        private static bool _initialized = false;
        private static bool _haveDataFromFile = false;

        /// <summary>
        /// Get the settings.
        /// </summary>
        /// <returns>The <see cref="SettingsData"/> object containting current settings.</returns>
        /// <exception cref="InvalidOperationException">Thrown if this is used before settings are loaded.</exception>
        public static SettingsData GetSettings()
        {
            if (!_initialized)
                throw new InvalidOperationException("Settings are not initialized!");

            return _settings;
        }

        /// <summary>
        /// Attempt to load settings data from file.
        /// </summary>
        public static void Load()
        {
            if (_initialized)
                return;

            if (File.Exists(FILE_NAME))
            {
                string cfgFile;

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
                    Type settingsDataType = typeof(SettingsData);
                    cfgFile = cfgFile.ReplaceLineEndings();
                    string[] lines = cfgFile.Split(Environment.NewLine);

                    foreach (string line in lines)
                    {
                        string[] parts = line.Split("=");
                        if (parts.Length != 2)
                            continue;
                        string inProperty = parts[0].Trim();
                        string inValue = parts[1].Trim();
                        if (inProperty.Length > 0 && inValue.Length > 0)
                        {
                            var property = settingsDataType.GetProperty(inProperty);
                            if (property != null)
                            {
                                Type propType = property.PropertyType.GetGenericArguments()[0];
                                var propValue = property.GetValue(_settings);
                                if (propValue != null)
                                {
                                    if (propType == typeof(string))
                                        ((Setting<string>)propValue).Value = inValue;
                                    else if (propType == typeof(bool))
                                        ((Setting<bool>)propValue).Value = Boolean.Parse(inValue);
                                    else if (propType == typeof(int))
                                        ((Setting<int>)propValue).Value = Int32.Parse(inValue);
                                    else if (propType == typeof(KeyBind))
                                        ((Setting<KeyBind>)propValue).Value = KeyBind.FromPackedInt(UInt32.Parse(inValue));
                                    else
                                        throw new NotImplementedException(String.Format("Deserialization of type {0} is not implemented!", propType.Name));
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error while parsing settings data!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _haveDataFromFile = true;
            }

            _initialized = true;
        }

        /// <summary>
        /// Safe settings to file.
        /// </summary>
        public static void Safe()
        {
            try
            {
                PropertyInfo[] properties = typeof(SettingsData).GetProperties();
                StringBuilder outBuilder = new();

                foreach (PropertyInfo property in properties)
                {
                    Type propType = property.PropertyType.GetGenericArguments()[0];
                    var val = property.GetValue(_settings);
                    if (val != null)
                    {
                        if (propType == typeof(string))
                            outBuilder.AppendFormat("{0}=\"{1}\"{2}", property.Name, ((Setting<string>)val).Value, Environment.NewLine);
                        else if (propType == typeof(bool))
                            outBuilder.AppendFormat("{0}={1}{2}", property.Name, ((Setting<bool>)val).Value, Environment.NewLine);
                        else if (propType == typeof(int))
                            outBuilder.AppendFormat("{0}={1}{2}", property.Name, ((Setting<int>)val).Value, Environment.NewLine);
                        else if (propType == typeof(KeyBind))
                            outBuilder.AppendFormat("{0}={1}{2}", property.Name, ((Setting<KeyBind>)val).Value.PackInt(), Environment.NewLine);
                        else
                            throw new NotImplementedException(String.Format("Serialization of type {0} is not implemented!", propType.Name));
                    }
                }

                File.WriteAllText(FILE_NAME, outBuilder.ToString());
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
