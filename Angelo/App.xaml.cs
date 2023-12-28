﻿using System.Windows;
using Angelo.Keybinds;
using Angelo.Settings;

namespace Angelo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            SettingsManager.Load();
            KeyBindManager.GetInstance().LoadSettings();
            MainWindow main = new();
            main.Show();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            SettingsManager.Safe();
        }
    }
}