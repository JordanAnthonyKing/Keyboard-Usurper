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

		// TODO: Remove this
		private vkCode _activationKey = vkCode.VK_SPACE;
		private vkCode _savedKeyDown = vkCode.VK_NULL;

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
			switch (_stateMachine.CurrentAction)
			{
				case Action.DiscardKey:
					return DiscardKey();
				case Action.TapActivationKey:
					return TapActivationKey(code);
				case Action.DelayKeyDown:
					return DelayKeyDown(code);
				case Action.MapKeyUp:
					return MapKeyUp(code);
				case Action.MapKeyDown:
					return MapKeyDown(code);
				case Action.ActivationKeyDownThenKey:
					return ActivationKeyDownThenKey(code);
				case Action.EmitActDownSavedDownActUp:
					return EmitActDownSavedDownActUp(code);
				case Action.MapSavedAndMapCurrentDown:
					return MapSavedAndMapCurrentDown(code);
				case Action.MapSavedAndMapCurrentUp:
					return MapSavedAndMapCurrentUp(code);
				case Action.EmitActSavedAndCurrentDown:
					return EmitActSavedAndCurrentDown(code);
				case Action.EmitActSavedAndCurrentUp:
					return EmitActSavedAndCurrentUp(code);
				case Action.EmitSavedDownAndActUp:
					return EmitSavedDownAndActUp(code);
				case Action.EmitSavedAndCurrentDown:
					return EmitSavedAndCurrentDown(code);
				case Action.DiscardKeyAndReleaseMappedKeys:
					return DiscardKeyAndReleaseMappedKeys(code);
				case Action.RunConfigure:
					// return RunConfigure();
					break;
				case Action.Null:
					// Do nothing
					break;
			}

			return false;
		}

		// TODO: Remove or change this
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

		private bool MapKey(vkCode code, bool up)
		{
			vkCode newCode = TranslateCode(code);

			if (newCode != vkCode.VK_NULL)
			{
				// TODO: Some logic
				KeyEvent(newCode, up);
				return true;
			}
			return false;
		}

		private bool DiscardKey() => true;

		private bool DiscardKeyAndReleaseMappedKeys(vkCode code) 
		{
			// TODO: 'Mapped' keys logic

			return true;	
		}

		private bool TapActivationKey(vkCode code)
		{
			TapKey(_activationKey);
			return true;
		}

		private bool ActivationKeyDownThenKey(vkCode code)
		{
			KeyDown(_activationKey);
			KeyDown(code);
			return true;
		}

		private bool MapKeyDown(vkCode code)
		{
			return MapKey(code, false);
		}

		private bool MapKeyUp(vkCode code)
		{
			return MapKey(code, false);
		}

		private bool DelayKeyDown(vkCode code)
		{
			// touchcursor has some assert logic here to make sure
			// the program doesn't get stuck in a bad state
			// but I think it will be unneeded here

			_savedKeyDown = code;
			return true;
		}

		private bool EmitSaved(vkCode code)
		{
			if (_savedKeyDown != vkCode.VK_NULL)
			{
				KeyDown(code);
				_savedKeyDown = vkCode.VK_NULL;
			}
			return true;
		}

		private bool EmitActDownSavedDownActUp(vkCode code) 
		{
			KeyDown(_activationKey);
			EmitSaved(code);
			KeyUp(_activationKey);
			return true;
		}

		private bool EmitSavedDownAndActUp(vkCode code) {
			EmitSaved(code);
			KeyUp(_activationKey);
			return true;
		}

		private void MapSavedDown() 
		{
			if (_savedKeyDown != vkCode.VK_NULL)
			{
				MapKeyDown(_savedKeyDown);
				_savedKeyDown = vkCode.VK_NULL;
			}
		}

		private bool MapSavedAndMapCurrentDown(vkCode code) 
		{
			MapSavedDown();
			MapKeyDown(code);
			return true;
		}

		private bool MapSavedAndMapCurrentUp(vkCode code) {
			MapSavedDown();
			MapKeyUp(code);
			return true;
		}

		private bool EmitActSavedAndCurrentDown(vkCode code) {
			KeyDown(_activationKey);
			EmitSaved(code);
			KeyDown(code);
			return true;
		}

		private bool EmitActSavedAndCurrentUp(vkCode code) {
			KeyDown(_activationKey);
			EmitSaved(code);
			KeyUp(code);
			return true;
		}

		private bool EmitSavedAndCurrentDown(vkCode code) {
			EmitSaved(code);
			KeyDown(code);
			return true;
		}

		private void TapKey(vkCode code)
		{
			KeyDown(code);
			KeyUp(code);
		}

		private void KeyDown(vkCode code)
		{
			KeyEvent(code, false);
		}

		private void KeyUp(vkCode code)
		{
			KeyEvent(code, true);
		}

		private void KeyEvent(vkCode code, bool up)
		{
			if (!up)
			{
				// TODO: Add modifiers
			}
			SendInput(code, up);
			if (!up)
			{
				// Something else?
			}
		}

		// TODO
		private vkCode TranslateCode(vkCode code)
		{
			return vkCode.VK_NULL;
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
