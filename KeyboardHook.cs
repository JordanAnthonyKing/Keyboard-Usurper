using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Windows.Sdk;
using System.Runtime.InteropServices;
using System.Reflection;
using Microsoft.Win32.SafeHandles;

namespace Keyboard_Usurper
{
	public class KeyboardHook
	{
		private UnhookWindowsHookExSafeHandle _hookHandle = null;
		private HOOKPROC _hookFunction = null;
		private Mapping _mapping;

		public KeyboardHook(Mapping mapping)
		{
			_hookFunction = new HOOKPROC(HookCallBack);
			_mapping = mapping;
			Install();
		}

		// private LRESULT HookCallBack(int code, IntPtr wParam, ref KBDLLHOOKSTRUCT lParam)
		private LRESULT HookCallBack(int nCode, WPARAM wParam, LPARAM lParam)
		{
			if (nCode == Constants.HC_ACTION)
			{
				KBDLLHOOKSTRUCT kbd = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
				System.Diagnostics.Debug.WriteLine(kbd.vkCode);
				vkCode code = (vkCode)kbd.vkCode;
				System.Diagnostics.Debug.WriteLine(code);

				KeyToKey k2k = _mapping.Mappings.Find(x => (uint)x.From == kbd.vkCode);
				if (k2k != null)
				{
					INPUT[] inputs = new INPUT[2];
					inputs[0].type = INPUT_typeFlags.INPUT_KEYBOARD;
					inputs[0].Anonymous.ki.wVk = (ushort)vkCode.VK_NUMPAD9;
					inputs[1].type = INPUT_typeFlags.INPUT_KEYBOARD;
					inputs[1].Anonymous.ki.wVk = (ushort)vkCode.VK_NUMPAD9;
					inputs[1].Anonymous.ki.dwFlags = keybd_eventFlags.KEYEVENTF_KEYUP;

					PInvoke.SendInput(new Span<INPUT>(inputs), Marshal.SizeOf<INPUT>());
					return (LRESULT)1;
				}
			}
			return PInvoke.CallNextHookEx(_hookHandle, nCode, wParam, lParam);

		}

		private void Install()
		{
			// make sure not already installed
			if (_hookHandle != null)
				return;

			// _hookHandle = PInvoke.SetWindowsHookEx(
			// 	SetWindowsHookEx_idHook.WH_KEYBOARD_LL,
			// 	_hookFunction,
			// 	new SafeProcessHandle(PInvoke.GetModuleHandle((string)null), true),
			// 	0);

			_hookHandle = PInvoke.SetWindowsHookEx(
			 	SetWindowsHookEx_idHook.WH_KEYBOARD_LL,
			 	_hookFunction,
			 	null,
			 	0);

			System.Diagnostics.Debug.WriteLine("Something to break on");
		}

		public void Uninstall()
		{
			if (_hookHandle != null)
			{
				// uninstall system-wide hook
				_hookHandle.Dispose();
			}
		}
	}
}
