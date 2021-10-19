using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Windows.Sdk;
using System.Runtime.InteropServices;

namespace Keyboard_Usurper
{
	public class KeyboardHook
	{
		private UnhookWindowsHookExSafeHandle _hookHandle = null;
		private HOOKPROC _hookProc;
		private List<KeyToKey> _mappings;
		private StateMachine _activeStateMachine = null;

		private readonly List<StateMachine> _stateMachines = new List<StateMachine>();

		public KeyboardHook(List<KeyToKey> mappings)
		{
			_mappings = mappings;
			_hookProc =  new HOOKPROC(HookCallBack);

			List<vkCode> usedMods = new List<vkCode>();

			List<vkCode> extraMods = new();
			_mappings.ForEach(x =>
			{
				vkCode activationKey = x.From.ActivationKey;
				if (activationKey != vkCode.VK_NULL && !usedMods.Contains(activationKey))
				{
					usedMods.Add(activationKey);
					var newMachine = new StateMachine(
					  activationKey,
					  _mappings.Where(x => x.From.ActivationKey == activationKey).ToList()
					  );
					newMachine.SendInput += SendInputEventHandler;
					newMachine.Deactivate += DeactivateActiveMachine;

					_stateMachines.Add(newMachine);
				}
			});

			Install();
		}

		private LRESULT HookCallBack(int nCode, WPARAM wParam, LPARAM lParam)
		{
			if (nCode == Constants.HC_ACTION)
			{
				KBDLLHOOKSTRUCT kbd = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
				var code = (vkCode)kbd.vkCode;
				System.Diagnostics.Debug.WriteLine(code);
				var swallow = ProcessKey(wParam, (vkCode)kbd.vkCode);
				System.Diagnostics.Debug.WriteLine(swallow ? "swallow" : "call next");
				if (swallow)
                {
					// Swallow
					System.Diagnostics.Debug.WriteLine("Swallowed");
					return (LRESULT)1;
                }
			}
			// Don't swallow
			System.Diagnostics.Debug.WriteLine("Called next");
			return PInvoke.CallNextHookEx(_hookHandle, nCode, wParam, lParam);
		} 

		private bool ProcessKey(WPARAM wParam, vkCode code)
		{
			StateMachine machine;
			var keyDown = IsKeyDown(wParam);
			System.Diagnostics.Debug.WriteLine(keyDown ? "down" : "up");

			if (_activeStateMachine != null)
            {
				return _activeStateMachine.ProcessKey(IsKeyDown(wParam), code);
            } 
			else if ((machine = _stateMachines.Find(x => x.ActivationKey == code)) != null)
            {
				_activeStateMachine = machine;
				return machine.ProcessKey(IsKeyDown(wParam), code);
            }
			else
            {
				// Normal mappings go here
            }

			// return ProcessEvent(e, code);
			return false;
		}

		private void DeactivateActiveMachine(Object sender, EventArgs e)
        {
			_activeStateMachine = null;
        }

		private void SendInputEventHandler(Object sender, KeyCodeEvent e) 
			=> SendInput(e.code, e.up);

		private void SendInput(vkCode code, bool up = false)
		{
			INPUT[] inputs = new INPUT[1];
			inputs[0].type = INPUT_typeFlags.INPUT_KEYBOARD;
			inputs[0].Anonymous.ki.wVk = (ushort)code;
			if (up) inputs[0].Anonymous.ki.dwFlags = keybd_eventFlags.KEYEVENTF_KEYUP;

			PInvoke.SendInput(new Span<INPUT>(inputs), Marshal.SizeOf<INPUT>());
		}

		private bool IsKeyDown(WPARAM wParam)
		{
			return (nuint)wParam == Constants.WM_KEYDOWN || 
				(nuint)wParam == Constants.WM_SYSKEYDOWN;
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
