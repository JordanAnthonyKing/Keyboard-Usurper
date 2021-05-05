using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Windows.Sdk;
using System.Runtime.InteropServices;
using System.Reflection;
using Microsoft.Win32.SafeHandles;
using System.Threading;

namespace Keyboard_Usurper
{
	public class KeyboardHook
	{
		private UnhookWindowsHookExSafeHandle _hookHandle = null;
		private HOOKPROC _hookProc;
		private Mapping _mapping;
		// TODO: Preparse to remove any rebound mods
		private vkCode[] _defaultMods = new vkCode[]
		{
			vkCode.VK_LSHIFT,
			vkCode.VK_RSHIFT,
			vkCode.VK_LCONTROL,
			vkCode.VK_RCONTROL,
			vkCode.VK_LMENU,
			vkCode.VK_RMENU,
			vkCode.VK_LWIN,
			vkCode.VK_RWIN
		};
		// TODO: Handle leftshift + rightshift etc. together
		private vkCode[] _extraMods;
		private vkCode[] _mods { get => _defaultMods.Concat(_extraMods).ToArray(); }
		private List<vkCode> _heldExtraMods = new();
		private List<vkCode> _expectedInputs = new();
		private int _keyPressed = 0;

		public KeyboardHook(Mapping mapping)
		{
			_mapping = mapping;
			_hookProc =  new HOOKPROC(HookCallBack);

			List<vkCode> extraMods = new();
			_mapping.Mappings.ForEach(x =>
			{
				foreach(vkCode mod in x.From.Mods)
				{
					if (mod != vkCode.VK_SHIFT &&
						mod != vkCode.VK_CONTROL &&
						mod != vkCode.VK_MENU &&
						mod != vkCode.VK_WIN &&
						!extraMods.Contains(mod))
					{
						extraMods.Add(mod);
					}
				}
			});
			_extraMods = extraMods.ToArray();
			Install();
		}

		private LRESULT HookCallBack(int nCode, WPARAM wParam, LPARAM lParam)
		{
			if (nCode == Constants.HC_ACTION)
			{
				KBDLLHOOKSTRUCT kbd = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
				// TODO: Change this logic to be a stack
				if (_expectedInputs.Contains((vkCode)kbd.vkCode))
				{
					_expectedInputs.Remove((vkCode)kbd.vkCode);
					return PInvoke.CallNextHookEx(_hookHandle, nCode, wParam, lParam);
				}

				if ((nuint)wParam == Constants.WM_KEYDOWN)
					return DownEvent((vkCode)kbd.vkCode) ? (LRESULT)1 : PInvoke.CallNextHookEx(_hookHandle, nCode, wParam, lParam);
				if ((nuint)wParam == Constants.WM_KEYUP)
					return UpEvent((vkCode)kbd.vkCode) ? (LRESULT)1 : PInvoke.CallNextHookEx(_hookHandle, nCode, wParam, lParam);
			}

			return PInvoke.CallNextHookEx(_hookHandle, nCode, wParam, lParam);
		}

		private bool DownEvent(vkCode code)
		{
			// Maybe we should just keep a list of all held keys instead of treating it conditionally
			if (_extraMods.Contains(code))
			{
				_heldExtraMods.Add(code); 
				return true;
			}

			if (_heldExtraMods.Count > 0) _keyPressed++;
				
			return StandardBinding(code);
		}

		private bool UpEvent(vkCode code)
		{
			if (_extraMods.Contains(code) && _keyPressed > 0) {
				_heldExtraMods.RemoveAll(x => x == code);
				_keyPressed = 0;
				return true;
			}

			if (_extraMods.Contains(code) && _keyPressed == 0)
			{
				_heldExtraMods.RemoveAll(x => x == code);
				if (StandardBinding(code)) 
					return true;
				SendInput(code);
				SendInput(code, true);
				return true;
			}

			return false;
		}

		private bool StandardBinding(vkCode code)
		{
			vkCode[] heldMods = GetHeldMods();
			KeyToKey binding = _mapping.Mappings.Find(x => 
				x.From.Code == code &&
				x.From.Mods.Length == heldMods.Length &&
				heldMods.All(y => x.From.Mods.Contains(y))
			);

			if (binding != null)
			{
				ExecuteBinding(binding);
				return true;
			}

			return false;
		}

		private vkCode[] GetHeldMods()
		{
			List<vkCode> mods = new List<vkCode>();

			if(PInvoke.GetAsyncKeyState((int)vkCode.VK_LSHIFT) < 0 || PInvoke.GetAsyncKeyState((int)vkCode.VK_RSHIFT) < 0) { mods.Add(vkCode.VK_SHIFT); }
			if(PInvoke.GetAsyncKeyState((int)vkCode.VK_LCONTROL) < 0 || PInvoke.GetAsyncKeyState((int)vkCode.VK_RCONTROL) < 0) { mods.Add(vkCode.VK_CONTROL); }
			if(PInvoke.GetAsyncKeyState((int)vkCode.VK_LWIN) < 0 || PInvoke.GetAsyncKeyState((int)vkCode.VK_RWIN) < 0) { mods.Add(vkCode.VK_WIN); }
			if(PInvoke.GetAsyncKeyState((int)vkCode.VK_LMENU) < 0 || PInvoke.GetAsyncKeyState((int)vkCode.VK_RMENU) < 0) { mods.Add(vkCode.VK_MENU); }

			return mods.Concat(_heldExtraMods).ToArray();
		}

		// TODO: Clean up this behaviour
		private vkCode[] GetRealHeldMods()
		{
			List<vkCode> mods = new List<vkCode>();

			if(PInvoke.GetAsyncKeyState((int)vkCode.VK_LSHIFT) < 0 ) { mods.Add(vkCode.VK_LSHIFT); }
			if(PInvoke.GetAsyncKeyState((int)vkCode.VK_RSHIFT) < 0 ) { mods.Add(vkCode.VK_RSHIFT); }
			if(PInvoke.GetAsyncKeyState((int)vkCode.VK_LCONTROL) < 0 ) { mods.Add(vkCode.VK_LCONTROL); }
			if(PInvoke.GetAsyncKeyState((int)vkCode.VK_RCONTROL) < 0 ) { mods.Add(vkCode.VK_RCONTROL); }
			if(PInvoke.GetAsyncKeyState((int)vkCode.VK_LWIN) < 0 ) { mods.Add(vkCode.VK_LWIN); }
			if(PInvoke.GetAsyncKeyState((int)vkCode.VK_RWIN) < 0 ) { mods.Add(vkCode.VK_RWIN); }
			if(PInvoke.GetAsyncKeyState((int)vkCode.VK_LMENU) < 0 ) { mods.Add(vkCode.VK_LMENU); }
			if(PInvoke.GetAsyncKeyState((int)vkCode.VK_RMENU) < 0 ) { mods.Add(vkCode.VK_RMENU); }

			// return mods.Concat(_heldExtraMods).ToArray();
			return mods.ToArray();
		}

		private void ExecuteBinding(KeyToKey binding)
		{
			vkCode[] heldMods = GetRealHeldMods();

			foreach (vkCode mod in heldMods)
				SendInput(mod, true);

			foreach (vkCode mod in binding.To.Mods)
				SendInput(mod);

			SendInput(binding.To.Code);
			SendInput(binding.To.Code, true);

			foreach (vkCode mod in binding.To.Mods)
				SendInput(mod, true);

			foreach (vkCode mod in heldMods)
				SendInput(mod);
		}

		private void SendInput(vkCode code, bool up = false)
		{
			INPUT[] inputs = new INPUT[1];
			inputs[0].type = INPUT_typeFlags.INPUT_KEYBOARD;
			inputs[0].Anonymous.ki.wVk = (ushort)code;
			if (up) inputs[0].Anonymous.ki.dwFlags = keybd_eventFlags.KEYEVENTF_KEYUP;

			_expectedInputs.Add(code);
			PInvoke.SendInput(new Span<INPUT>(inputs), Marshal.SizeOf<INPUT>());
		}

		private void Install()
		{
			if (_hookHandle != null) return;

			_hookHandle = PInvoke.SetWindowsHookEx(
			 	SetWindowsHookEx_idHook.WH_KEYBOARD_LL,
				_hookProc,
			 	null,
			 	0);
		}

		public void Uninstall()
		{
			_hookHandle?.Dispose();
		}
	}
}
