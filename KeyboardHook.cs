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
		private LRESULT HookCallBack(int code, WPARAM wParam, LPARAM lParam)
		{
			if (code < 0) return PInvoke.CallNextHookEx(_hookHandle, code, wParam, lParam);

			// KeyUp event
			// if ((lParam.flags & 0x80) != 0 && this.KeyUp != null)
			// this.KeyUp(this, new HookEventArgs(lParam.vkCode));
			if (code == 0 && wParam.Value == Constants.WM_KEYDOWN) 
			{ 
				System.Diagnostics.Debug.WriteLine("Key down event"); 
			} 

			// KeyDown event
			// if ((lParam.flags & 0x80) == 0 && this.KeyDown != null)
			// this.KeyDown(this, new HookEventArgs(lParam.vkCode));
			if (code == 0 && wParam.Value == Constants.WM_KEYUP) 
			{ 
				System.Diagnostics.Debug.WriteLine("Key up event"); 
			} 

			System.Diagnostics.Debug.WriteLine("Hello World!");

			return PInvoke.CallNextHookEx(_hookHandle, code, wParam, lParam);

		}

		private void Install()
		{
			// make sure not already installed
			if (_hookHandle != null)
				return;

			_hookHandle = PInvoke.SetWindowsHookEx(
				SetWindowsHookEx_idHook.WH_KEYBOARD_LL,
				_hookFunction,
				new SafeProcessHandle(PInvoke.GetModuleHandle((string)null), true),
				0);

			System.Diagnostics.Debug.WriteLine("Something to break on");
		}

		private void Uninstall()
		{
			if (_hookHandle != null)
			{
				// uninstall system-wide hook
				_hookHandle.Dispose();
			}
		}
	}
}
