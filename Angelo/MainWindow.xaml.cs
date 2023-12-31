﻿using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using Angelo.Keybinds;
using Angelo.Settings;

namespace Angelo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly KeyBindManager _bindManager;
        private readonly SettingsData _settings;
        private readonly Harbormaster _harbormaster;

        public MainWindow()
        {
            _bindManager = new KeyBindManager();
            _settings = SettingsManager.GetSettings();
            InitializeComponent();
            LoadSettings();
            _harbormaster = new Harbormaster(this, new UI.StartButton(StartButton));
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _harbormaster.StopFishing();
            DebugWindow.GetInstance().Close();
        }

        /// <summary>
        /// Restore saved values for controls.
        /// </summary>
        private void LoadSettings()
        {
            SensSlider.Value = _settings.Sensitivity;
            ThresSlider.Value = _settings.Threshold;
            LureCheckbox.IsChecked = _settings.UseLure;
            FishingKeyInput.Text = _settings.FishingKey.ToString();
            LureKeyInput.Text = _settings.LureKey.ToString();

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
                _settings.Sensitivity = (int)SensSlider.Value;
        }

        private void ThresSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.OldValue != 0)
                _settings.Threshold = (int)ThresSlider.Value;
        }

        private void FishingKeyInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (_bindManager.SetKey(KeyBindId.FISHING, e.Key))
                FishingKeyInput.Text = _settings.FishingKey.ToString();
        }

        private void LureKeyInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (_bindManager.SetKey(KeyBindId.LURE, e.Key))
                LureKeyInput.Text = _settings.LureKey.ToString();
        }

        private void KeyInput_KeyUp(object sender, KeyEventArgs e)
        {
            _bindManager.UpdateModifier(e.Key, false);
        }

        private void LureCheckbox_Click(object sender, RoutedEventArgs e)
        {
            _settings.UseLure = LureCheckbox.IsChecked == true;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (_harbormaster.IsFishing())
                _harbormaster.StopFishing();
            else
                _harbormaster.StartFishing();
        }

        private void WAButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Properties.Resources.WA);
            AddLogLine("WeakAura import string copied to clipboard.");
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Couldn't open URL!", MessageBoxButton.OK, MessageBoxImage.Error);
                if (ex.StackTrace != null)
                    AddLogLine(ex.StackTrace);
            }
        }

        private void DBGButton_Click(object sender, RoutedEventArgs e)
        {
            DebugWindow debgWindow = DebugWindow.GetInstance();
            debgWindow.Show();
            debgWindow.Focus();
        }
    }
}
