using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Keyboard_Usurper
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{

		private KeyboardHook _hook;

        [STAThread]
		private void Usurp(object sender, StartupEventArgs e)
		{
			_hook = new KeyboardHook();

			MainWindow main = new MainWindow();
			main.Show();
		}		

		private void Unsurp(object sender, ExitEventArgs e)
		{
			_hook.Uninstall();
		}

	}
}
