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

namespace Keyboard_Usurper
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : System.Windows.Application
    {
		private KeyboardHook _hook;
		// Magic never read code that works
        private NotifyIcon _notifyIcon;
		private event EventHandler _test;

        [STAThread]
		private void Usurp(object sender, StartupEventArgs e)
		{
			string jsonString = File.ReadAllText(Path.Combine(Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%"), "usurper.json")); 
			Configuration config = JsonSerializer.Deserialize<Configuration>(jsonString);

			_hook = new KeyboardHook(ConfigurationToMapping.Convert(config));

			// _test += TestEventHandler;

            var contextMenuStrip = new ContextMenuStrip();
			contextMenuStrip.Items.Add("Usurped", null, TestEventHandler);

			_notifyIcon = new NotifyIcon
            {
                Text = "Keyboard Usurper",
                Icon = Keyboard_Usurper.Properties.Resources.crown,
                Visible = true,
				ContextMenuStrip = contextMenuStrip
            };

			// MainWindow main = new MainWindow();
			// main.Show();
		}		

		private void TestEventHandler(object sender, EventArgs e)
        {
			System.Diagnostics.Debug.WriteLine("working");
        }

		private void Unsurp(object sender, ExitEventArgs e)
		{
			_hook.Uninstall();
			_notifyIcon.Dispose();
		}
	}
}
