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
		private List<KeyCodeEvent> _expectedKeys = new List<KeyCodeEvent>();

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

			_mappings.RemoveAll(x => x.From.ActivationKey != vkCode.VK_NULL); 

			Install();
		}

		private LRESULT HookCallBack(int nCode, WPARAM wParam, LPARAM lParam)
		{
			if (nCode == Constants.HC_ACTION)
			{
				KBDLLHOOKSTRUCT kbd = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
				var code = (vkCode)kbd.vkCode;

				var expectedIndex = _expectedKeys.FindIndex(x => x.code == code && x.up == !IsKeyDown(wParam));
				if (expectedIndex < 0)
                {
                    // System.Diagnostics.Debug.WriteLine(code);
                    var swallow = ProcessKey(wParam, (vkCode)kbd.vkCode);
                    // System.Diagnostics.Debug.WriteLine(swallow ? "swallow" : "call next");
                    if (swallow)
                    {
                        // Swallow
                        // System.Diagnostics.Debug.WriteLine("Swallowed");
                        return (LRESULT)1;
                    }
                }
				else
                {
					_expectedKeys.RemoveAt(expectedIndex);
                }
			}
			// Don't swallow
			// System.Diagnostics.Debug.WriteLine("Called next");
			return PInvoke.CallNextHookEx(_hookHandle, nCode, wParam, lParam);
		} 

		private bool ProcessKey(WPARAM wParam, vkCode code)
		{
			StateMachine machine;
			var keyDown = IsKeyDown(wParam);
			// System.Diagnostics.Debug.WriteLine(keyDown ? "down" : "up");

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
				if (IsKeyDown(wParam))
                {
					KeyToKey binding = MatchBinding(code);
					if (binding != null)
                    {
						ExecuteBinding(binding);
						return true;
                    }
                }
            }

			// return ProcessEvent(e, code);
			return false;
		}

		private KeyToKey MatchBinding(vkCode code)
		{
			List<vkCode> activeMods = new List<vkCode>();

			if (code == vkCode.VK_J)
			{
				System.Diagnostics.Debug.WriteLine(PInvoke.GetAsyncKeyState((int)vkCode.VK_LCONTROL) < 0);
			}

			// TODO: We can turn this into an interation method for reuse
			if (PInvoke.GetAsyncKeyState((int)vkCode.VK_LCONTROL) < 0 || PInvoke.GetAsyncKeyState((int)vkCode.VK_RCONTROL) < 0)
				activeMods.Add(vkCode.VK_CONTROL);
			if (PInvoke.GetAsyncKeyState((int)vkCode.VK_LSHIFT) < 0 || PInvoke.GetAsyncKeyState((int)vkCode.VK_RSHIFT) < 0)
				activeMods.Add(vkCode.VK_SHIFT);
			if (PInvoke.GetAsyncKeyState((int)vkCode.VK_LMENU) < 0 || PInvoke.GetAsyncKeyState((int)vkCode.VK_RMENU) < 0)
				activeMods.Add(vkCode.VK_MENU);
			if (PInvoke.GetAsyncKeyState((int)vkCode.VK_LWIN) < 0 || PInvoke.GetAsyncKeyState((int)vkCode.VK_RWIN) < 0)
				activeMods.Add(vkCode.VK_WIN);

			// Exact match
			var toReturn = _mappings.Find(x => 
                x.From.Code == code && 
                x.From.Mods.All(y => activeMods.Contains(y)) &&
                x.From.Mods.Count() == activeMods.Count // This may need rewriting when we separate L and R mods
			);
			if (toReturn != null) return toReturn;

			// The same mods are active
			return _mappings.Find(x => x.From.Code == code && x.From.Mods.All(y => activeMods.Contains(y)));
		}

		private void ExecuteBinding(KeyToKey binding)
        {
			List<vkCode> activeMods = new List<vkCode>();
			// TODO: Use the reiteration method spoken about above
			if (PInvoke.GetAsyncKeyState((int)vkCode.VK_LCONTROL) < 0)
				activeMods.Add(vkCode.VK_LCONTROL);
			if (PInvoke.GetAsyncKeyState((int)vkCode.VK_RCONTROL) < 0)
				activeMods.Add(vkCode.VK_RCONTROL);
			if (PInvoke.GetAsyncKeyState((int)vkCode.VK_LSHIFT) < 0)
				activeMods.Add(vkCode.VK_LSHIFT);
			if (PInvoke.GetAsyncKeyState((int)vkCode.VK_RSHIFT) < 0)
				activeMods.Add(vkCode.VK_LSHIFT);
			if (PInvoke.GetAsyncKeyState((int)vkCode.VK_LMENU) < 0)
				activeMods.Add(vkCode.VK_LMENU);
			if (PInvoke.GetAsyncKeyState((int)vkCode.VK_RMENU) < 0)
				activeMods.Add(vkCode.VK_RMENU);
			if (PInvoke.GetAsyncKeyState((int)vkCode.VK_LWIN) < 0)
				activeMods.Add(vkCode.VK_LWIN);
			if (PInvoke.GetAsyncKeyState((int)vkCode.VK_RWIN) < 0)
				activeMods.Add(vkCode.VK_LWIN);

			if(!binding.To.WithMods)
                foreach (vkCode code in activeMods)
                    SendInput(code, true);

			if(binding.To.WithMods)
                foreach (vkCode code in binding.From.Mods)
                    SendInput(code, true);

			foreach (vkCode code in binding.To.Mods)
				SendInput(code);

			SendInput(binding.To.Code);
			SendInput(binding.To.Code, true);

			foreach (vkCode code in binding.To.Mods)
				SendInput(code, true);

			if(!binding.To.WithMods)
                foreach (vkCode code in activeMods)
                    SendInput(code);

			if(binding.To.WithMods)
                foreach (vkCode code in binding.From.Mods)
                    SendInput(code);
        }

		private void DeactivateActiveMachine(Object sender, EventArgs e) =>
			_activeStateMachine = null;

		private void SendInputEventHandler(Object sender, KeyCodeEvent e)
        {
			_expectedKeys.Add(e);
			SendInput(e.code, e.up);
        }

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
