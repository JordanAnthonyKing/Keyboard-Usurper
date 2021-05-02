using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keyboard_Usurper
{
	public class Configuration
	{
		public ConfigBinding[] bindings { get; set; }
	}

	public class ConfigBinding
	{
		public string from { get; set; }
		public string to { get; set; }
	}
}
