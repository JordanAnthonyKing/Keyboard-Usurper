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
		private Mapping _mapping;
		private vkCode[] _mods = new vkCode[]{ 
			vkCode.VK_LSHIFT,
			vkCode.VK_RSHIFT,
			vkCode.VK_LWIN,
			vkCode.VK_RWIN,
			vkCode.VK_LCONTROL,
			vkCode.VK_RCONTROL,
			vkCode.VK_LMENU,
			vkCode.VK_RMENU
		};
		private List<vkCode> _expectedInputs = new();

		private readonly List<StateMachine> _stateMachines = new List<StateMachine>();

		// This logic should really be moved out of the Keyboard hook
		public KeyboardHook(Mapping mapping)
		{
			_mapping = mapping;
			_hookProc =  new HOOKPROC(HookCallBack);

			List<vkCode> usedMods = new List<vkCode>();

			List<vkCode> extraMods = new();
			_mapping.Mappings.ForEach(x =>
			{
				vkCode activationKey = x.From.ActivationKey;
				if (activationKey != vkCode.VK_NULL && !usedMods.Contains(activationKey))
				{
					usedMods.Add(activationKey);
					_stateMachines.Add(new StateMachine(
					  activationKey, 
					  new Mapping { Mappings = _mapping.Mappings.Where(x => x.From.ActivationKey == activationKey).ToList() }
					));
				}
			});

			Install();
		}

		private LRESULT HookCallBack(int nCode, WPARAM wParam, LPARAM lParam)
		{
			if (nCode == Constants.HC_ACTION)
			{
				KBDLLHOOKSTRUCT kbd = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
				ProcessKey(wParam, (vkCode)kbd.vkCode);
			}

			return PInvoke.CallNextHookEx(_hookHandle, nCode, wParam, lParam);
		}

		private bool ProcessKey(WPARAM wParam, vkCode code)
		{
			// TODO: If it's a normal mod and the mod isn't remapped we need to skip over it?
			// A normal mod will skip over itself by falling out of the logic.
			// Touch cursor expects only one activation key, we have many, therefore we need
			// to track them.
			// I don't know if we should do this with async checks or by the same as _mappedKeys


			// TODO: This needs writing to use some sort of list that contains state machine starters
			// And the logic needs (mostly) moving into the statemachines themselves
			Event e = Event.NumEvents;

			// I think really this code needs to go into the statemachine
			if (code == _activationKey)
			{
				e = IsKeyDown(wParam) ? Event.ActivationDown : Event.ActivationUp;
			}
			// else if (_mapping.Mappings.Exists(x => x.To.Code == code)) 
			else if (TranslateCode(code) != vkCode.VK_NULL) 
			{
				e = IsKeyDown(wParam) ? Event.MappedKeyDown : Event.MappedKeyUp;
			}
			else
			{
				e = IsKeyDown(wParam) ? Event.OtherKeyDown : Event.OtherKeyUp;
			}
			// State machine
			return ProcessEvent(e, code);
		}


		// TODO: Remove or change this
		private void SendInput(vkCode code, bool up = false)
		{
			INPUT[] inputs = new INPUT[1];
			inputs[0].type = INPUT_typeFlags.INPUT_KEYBOARD;
			inputs[0].Anonymous.ki.wVk = (ushort)code;
			if (up) inputs[0].Anonymous.ki.dwFlags = keybd_eventFlags.KEYEVENTF_KEYUP;

			// TODO: Double check if this is needed
			_expectedInputs.Add(code);
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
