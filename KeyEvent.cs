using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keyboard_Usurper
{
	public class KeyEvent
	{
		// TODO: Make this an enum probably
		public uint Message;
		public vkCode Key;
		// TODO: Do I need this?
		public uint extraInfo;
	}
}
