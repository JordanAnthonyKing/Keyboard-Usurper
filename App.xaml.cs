using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using System.Windows.Forms;
using Hardcodet.Wpf.TaskbarNotification;

namespace Keyboard_Usurper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private KeyboardHook _hook;
        private TaskbarIcon notifyIcon;

        [STAThread]
        private void Usurp(object sender, StartupEventArgs e)
        {
            string jsonString = File.ReadAllText(Path.Combine(Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%"), "usurper.json"));
            Configuration config = JsonSerializer.Deserialize<Configuration>(jsonString);

            _hook = new KeyboardHook(ConfigurationToMapping.Convert(config));

            notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");

            // MainWindow main = new MainWindow();
            // main.Show();
        }

        private void Unsurp(object sender, ExitEventArgs e)
        {
            notifyIcon.Dispose();
            _hook.Uninstall();
        }
    }
}
