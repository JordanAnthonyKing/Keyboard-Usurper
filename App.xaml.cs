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
			_hook = new KeyboardHook(
				new Mapping
				{
					Mappings = new List<KeyToKey>()
					{
						new KeyToKey { From = vkCode.VK_NUMPAD0, To = vkCode.VK_NUMPAD9 }
					}
				}
			); ;

			MainWindow main = new MainWindow();
			main.Show();
		}		

		private void Unsurp(object sender, ExitEventArgs e)
		{
			_hook.Uninstall();
		}
	}
}
