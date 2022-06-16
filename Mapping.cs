using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keyboard_Usurper
{
	// TODO: Make most of this stuff readonly or get not set
	public class KeyToKey
	{
		public Key From;
		public Key To;
		// TODO: Figure out how to tie this to dynamic functions
		// Or we could just give each a set ID? and if no To then add that set ID
		// to disabled? Needs special case for the actual toggler...
		// public Func<bool> Enabled;
		// public Func<string, null> ToggleFunc;

	}

	public class Key
	{
		public IEnumerable<vkCode> Mods;
		public vkCode ActivationKey;
		public vkCode Code;
		public bool WithMods;
	}

	public static class ConfigurationToMapping
	{
		// TODO: Make this an array?
		public static List<KeyToKey> Convert(Configuration config)
		{
			List<KeyToKey> mappings = new();
			vkCode[] mods = new vkCode[]{ 
				vkCode.VK_LSHIFT,
				vkCode.VK_RSHIFT,
				vkCode.VK_SHIFT,
				vkCode.VK_LWIN,
				vkCode.VK_RWIN,
				vkCode.VK_WIN,
				vkCode.VK_LCONTROL,
				vkCode.VK_RCONTROL,
				vkCode.VK_CONTROL,
				vkCode.VK_LMENU,
				vkCode.VK_RMENU,
				vkCode.VK_MENU
			};

			// TODO: Handle ---
			foreach(ConfigBinding binding in config.bindings)
			{
				string[] fromKeys = binding.from.Split('-');
				string[] toKeys = binding.to.Split('-');

				KeyToKey k2k = new()
				{
					From = new Key
					{
						Mods = fromKeys
						  .Take(fromKeys.Length - 1)
						  .Select(x => StringToCode.ConvertTo(x))
						  .Where(x => mods.Contains(x)),
						// TODO: A better way to do this?
						ActivationKey = fromKeys
						  .Take(fromKeys.Length - 1)
						  .Select(x => StringToCode.ConvertTo(x))
						  .Where(x => !mods.Contains(x))
						  .FirstOrDefault(),
						Code = StringToCode.ConvertTo(fromKeys.Last())
					},
					To = new Key
					{
						// TODO: Some code to detect single mod keys
						Mods = toKeys
							.Take(toKeys.Length - 1)
							.Select(x => StringToCode.ConvertTo(x))
							.Where(x => mods.Contains(x)),
						Code = toKeys.Last()[0] == '+' ?
  						StringToCode.ConvertTo(toKeys.Last().Substring(1)) :
						StringToCode.ConvertTo(toKeys.Last()),
						WithMods = toKeys.Last()[0] == '+'
					}
				};

				mappings.Add(k2k);
			}

			return mappings;
		}
	}
}
