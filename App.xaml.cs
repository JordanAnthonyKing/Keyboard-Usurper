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
        // Magic never read code that works
        // private NotifyIcon _notifyIcon;
        // private ContextMenuStrip _contextMenuStrip = new();

        private TaskbarIcon notifyIcon;
        
        [STAThread]
        private void Usurp(object sender, StartupEventArgs e)
        {
            string jsonString = File.ReadAllText(Path.Combine(Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%"), "usurper.json"));
            Configuration config = JsonSerializer.Deserialize<Configuration>(jsonString);

            _hook = new KeyboardHook(ConfigurationToMapping.Convert(config));

            // _contextMenuStrip.Items.Add("Disable", null, TestEventHandler);

            // _notifyIcon = new NotifyIcon
            // {
            //     Text = "Keyboard Usurper",
            //     Icon = Keyboard_Usurper.Properties.Resources.crown,
            //     Visible = true,
            // 	   ContextMenuStrip = _contextMenuStrip,
            // };

            notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");

            // MainWindow main = new MainWindow();
            // main.Show();
        }

        private void TestEventHandler(object sender, EventArgs e)
        {
            // _hook.Active = !_hook.Active;
            // if (_hook.Active)
            // {
            //     _contextMenuStrip.Items.Clear();
            //     _contextMenuStrip.Items.Add("Disable", null, TestEventHandler);
            // }
            // else
            // {
            //     _contextMenuStrip.Items.Clear();
            //     _contextMenuStrip.Items.Add("Enable", null, TestEventHandler);
            // }
        }

        private void Unsurp(object sender, ExitEventArgs e)
        {
            notifyIcon.Dispose();
            _hook.Uninstall();
            // _notifyIcon.Dispose();
        }
    }
}
