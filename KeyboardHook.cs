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
	class KeyboardHook
	{
		UnhookWindowsHookExSafeHandle _hookHandle = null;
		HOOKPROC _hookFunction = null;

		// public delegate void HookEventHandler(object sender, HookEventArgs e);
		// public event HookEventHandler KeyDown;
		// public event HookEventHandler KeyUp;

		public KeyboardHook()
		{
			_hookFunction = new HOOKPROC(HookCallBack);
			Install();
		}

		// private LRESULT HookCallBack(int code, IntPtr wParam, ref KBDLLHOOKSTRUCT lParam)
		private LRESULT HookCallBack(int nCode, WPARAM wParam, LPARAM lParam)
		{
			try
			{
				if (nCode == Constants.HC_ACTION && (nuint)wParam == Constants.WM_KEYDOWN)
				{
					// System.Diagnostics.Debug.WriteLine("Event");
					
					try
					{
						KBDLLHOOKSTRUCT kbd = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
						System.Diagnostics.Debug.WriteLine(kbd);
						System.Diagnostics.Debug.WriteLine(kbd.vkCode);
						System.Diagnostics.Debug.WriteLine(kbd.dwExtraInfo);
					}
					catch (Exception ex)
					{
						System.Diagnostics.Debug.WriteLine("cast throw");
						System.Diagnostics.Debug.WriteLine(ex);
					}
					return (LRESULT)1;
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("big throw");
				System.Diagnostics.Debug.WriteLine(ex);
			}


			// if (nCode == 0 && (nuint)wParam == Constants.WM_KEYDOWN) 
			// { 
			// 	System.Diagnostics.Debug.WriteLine("Key down event"); 
			// } 

			// if (nCode == 0 && (nuint)wParam == Constants.WM_KEYUP) 
			// { 
			// 	System.Diagnostics.Debug.WriteLine("Key up event"); 
			// } 

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
