﻿using Angelo.Settings;
using System.Windows;

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
            MainWindow main = new();
            main.Show();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            SettingsManager.Safe();
        }
    }
}
