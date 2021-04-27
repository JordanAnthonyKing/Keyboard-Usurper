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
        [STAThread]
		private void Usurp(object sender, StartupEventArgs e)
		{
			MainWindow main = new MainWindow();
			main.Show();
		}		

	}
}
