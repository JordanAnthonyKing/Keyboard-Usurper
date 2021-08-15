using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keyboard_Usurper
{
	// TODO: Probably rename this to Mappings
	public class Mapping
	{
		public List<KeyToKey> Mappings = new();
	}

	public class KeyToKey
	{
		public Key From;
		public Key To;
	}

	public class Key
	{
		public vkCode[] Mods;
		public vkCode Code;
	}

	public static class ConfigurationToMapping
	{
		public static Mapping Convert(Configuration config)
		{
			Mapping mapping = new();

			// TODO: Handle ---
			foreach(ConfigBinding binding in config.bindings)
			{
				string[] fromKeys = binding.from.Split('-');
				string[] toKeys = binding.to.Split('-');

				KeyToKey k2k = new()
				{
					From = new Key
					{
						Mods = fromKeys.Take(fromKeys.Length - 1).Select(x => StringToCode.ConvertTo(x)).ToArray(),
						Code = StringToCode.ConvertTo(fromKeys.Last())
					},
					To = new Key
					{
						Mods = toKeys.Take(toKeys.Length - 1).Select(x => StringToCode.ConvertTo(x)).ToArray(),
						Code = StringToCode.ConvertTo(toKeys.Last())
					}
				};

				mapping.Mappings.Add(k2k);
			}

			return mapping;
		}
	}
}
