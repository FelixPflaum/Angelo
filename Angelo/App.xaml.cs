using System.Diagnostics;
using System.Windows;
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
            MainWindow main = new();
            main.Show();

#if DEBUG
            DebugWindow debgWindow = DebugWindow.GetInstance();
            debgWindow.Show();
            debgWindow.Focus();
#endif
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            SettingsManager.Safe();
        }
    }
}
