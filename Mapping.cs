using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keyboard_Usurper
{
	public class Mapping
	{
		public List<KeyToKey> Mappings;
	}

	public class KeyToKey
	{
		public vkCode From;
		public vkCode To;
	}
}
