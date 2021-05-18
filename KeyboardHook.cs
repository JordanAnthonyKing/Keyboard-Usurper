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
		private vkCode[] _extraMods;
		private List<vkCode> _expectedInputs = new();

		private readonly StateMachine _stateMachine = new StateMachine();

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
			}

			return PInvoke.CallNextHookEx(_hookHandle, nCode, wParam, lParam);
		}

		private bool ProcessKey(WPARAM wParam, vkCode code)
		{
			Event e = Event.NumEvents;

			if (_extraMods.Contains(code))
			{
				e = IsKeyDown(wParam) ? Event.ActivationDown : Event.ActivationUp;
			}
			else if (_mapping.Mappings.Exists(x => x.To.Code == code)) 
			{
				// This isn't really right yet because we have multiple mappings
				// Perhaps we need to presort the mappings or something
				e = IsKeyDown(wParam) ? Event.MappedKeyDown : Event.MappedKeyUp;
			}
			else
			{
				e = IsKeyDown(wParam) ? Event.OtherKeyDown : Event.OtherKeyUp;
			}

			// State machine
			return ProcessEvent(e, code);
		}

		private bool ProcessEvent(Event e, vkCode code)
		{
			if (_stateMachine.GetNext(e).NewState != State.Self)	
				_stateMachine.MoveNext(e);

			// TODO
			switch (_stateMachine.CurrentState)
			{
				case State.Idle:
					break;
				case State.WaitMappedDown:
					break;
				case State.WaitMappedDownSpaceEmitted:
					break;
				case State.WaitMappedUp:
					break;
				case State.WaitMappedUpSpaceEmitted:
					break;
				case State.Mapping:
					break;
				case State.NumStates:
					break;
				case State.Self:
					break;
				case State.NA:
					break;
			}

			return false;
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
