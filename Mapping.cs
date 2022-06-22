using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keyboard_Usurper
{
    public class KeyToKey
    {
        public Key From { get; }
        public Key To { get; }
        public int GroupId { get; }

        public KeyToKey(Key from, Key to, int groupId)
        {
            From = from;
            To = to;
            GroupId = groupId;
        }
    }

    public class Key
    {
        public IEnumerable<vkCode> Mods { get; }
        public vkCode ActivationKey { get; }
        public vkCode Code { get; }
        public bool WithMods { get; }

        public Key(IEnumerable<vkCode> mods, vkCode activationKey, vkCode code, bool withMods)
        {
            Mods = mods;
            ActivationKey = activationKey;
            Code = code;
            WithMods = withMods;
        }
    }

    public static class ConfigurationToMapping
    {
        // TODO: Make this an array?
        public static List<KeyToKey> Convert(Configuration config)
        {
            List<KeyToKey> mappings = new();
            vkCode[] modifiers = new vkCode[]{
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

            Func<string[], Key> createFromKey = delegate (string[] keys)
            {

                IEnumerable<vkCode> mods = keys
                      .Take(keys.Length - 1)
                      .Select(x => StringToCode.ConvertTo(x))
                      .Where(x => modifiers.Contains(x));
                // TODO: A better way to do this?
                vkCode activationKey = keys
                      .Take(keys.Length - 1)
                      .Select(x => StringToCode.ConvertTo(x))
                      .FirstOrDefault(x => !modifiers.Contains(x));
                vkCode code = StringToCode.ConvertTo(keys.Last());

                return new Key(mods, activationKey, code, false);
            };

            int count = 0;
            Func<int> getGroupId = delegate ()
            {
                count++;
                return count;
            };

            // TODO: Handle ---
            foreach (BindingSet set in config.bindings)
            {
                int groupId = getGroupId();

                mappings.Add(new KeyToKey(createFromKey(set.toggleBinding.Split('-')), null, groupId));

                foreach (ConfigBinding binding in set.bindings)
                {
                    string[] toKeys = binding.to.Split('-');

                    IEnumerable<vkCode> mods = toKeys
                        .Take(toKeys.Length - 1)
                        .Select(x => StringToCode.ConvertTo(x))
                        .Where(x => modifiers.Contains(x));
                    vkCode code = toKeys.Last()[0] == '+' ?
                        StringToCode.ConvertTo(toKeys.Last().Substring(1)) :
                        StringToCode.ConvertTo(toKeys.Last());
                    bool withMods = toKeys.Last()[0] == '+';

                    mappings.Add(new KeyToKey(createFromKey(binding.from.Split('-')), new Key(mods, vkCode.VK_NULL, code, withMods), groupId));
                }
            }

            return mappings;
        }
    }
}
