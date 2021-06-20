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
		private vkCode[] _extraMods;
		private List<vkCode> _expectedInputs = new();

		// TODO: Rewrite this for arbitrary keys
		private vkCode _activationKey = vkCode.VK_SPACE;
		private vkCode _savedKeyDown = vkCode.VK_NULL;

		private readonly StateMachine _stateMachine = new StateMachine();
		private readonly List<vkCode> _mappedKeysHeld = new List<vkCode>();

		public KeyboardHook(Mapping mapping)
		{
			_mapping = mapping;
			_hookProc =  new HOOKPROC(HookCallBack);

			List<vkCode> extraMods = new();
			_mapping.Mappings.ForEach(x =>
			{
				foreach(vkCode mod in x.From.Mods)
				{
					// I'm reconsidering the idea of having combined shift keys
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


			Event e = Event.NumEvents;

			if (_extraMods.Contains(code))
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

		private bool ProcessEvent(Event e, vkCode code)
		{
			if (_stateMachine.GetNext(e).NewState != State.Self)	
				_stateMachine.MoveNext(e);

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
				if (!up)
				{
					_mappedKeysHeld.Add(newCode);
				}	
				else
				{
					// This may need a try block
					_mappedKeysHeld.Remove(newCode);
				}
				
				KeyEvent(newCode, up);
				return true;
			}
			return false;
		}

		private bool DiscardKey() => true;

		private bool DiscardKeyAndReleaseMappedKeys(vkCode code) 
		{
			_mappedKeysHeld.ForEach(k =>
			{
				_mappedKeysHeld.ForEach(k =>
				{
					SendInput(k, false);
				});
				_mappedKeysHeld.Remove(k);
				KeyEvent(k, true);
			});
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
			if (!up) // Only add modifiers on key down
			{
				_mappedKeysHeld.ForEach(k =>
				{
					SendInput(k, false);
				});
			}
			SendInput(code, up);
			if (!up)
			{
				_mappedKeysHeld.ForEach(k =>
				{
					SendInput(k, false);
				});
			}
		}

		// TODO
		private vkCode TranslateCode(vkCode code)
		{
			// This is where all the mapping is done

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
