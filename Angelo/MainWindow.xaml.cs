using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Angelo.Settings;

namespace Angelo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadSettings();
        }

        /// <summary>
        /// Restore saved values for controls.
        /// </summary>
        private void LoadSettings()
        {
            SensSlider.Value = SettingsManager.settings.Sensitivity;
            ThresSlider.Value = SettingsManager.settings.Threshold;
            LureCheckbox.IsChecked = SettingsManager.settings.UseLure;

            if (SettingsManager.WereSettingsLoaded())
                AddLogLine("Settings restored from file!");
        }

        /// <summary>
        /// Add a line to the log box.
        /// </summary>
        /// <param name="line">The line to add.</param>
        public void AddLogLine(string line)
        {
            LogBox.AppendText(line + "\n");
            LogBox.ScrollToEnd();
        }

        private void SensSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.OldValue != 0)
            {
                SettingsManager.settings.Sensitivity = (int)SensSlider.Value;
            }
        }

        private void ThresSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.OldValue != 0)
            {
                SettingsManager.settings.Threshold = (int)ThresSlider.Value;
            }
        }

        private void FishingKeyInput_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void FishingKeyInput_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void LureKeyInput_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void LureKeyInput_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void LureCheckbox_Click(object sender, RoutedEventArgs e)
        {
            SettingsManager.settings.UseLure = LureCheckbox.IsChecked == true;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void WAButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {

        }
    }
}
