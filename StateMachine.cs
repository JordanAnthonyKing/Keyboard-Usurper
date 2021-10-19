using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Windows.Sdk;
using System.Runtime.InteropServices;

namespace Keyboard_Usurper
{
	public enum State
	{
		Idle,
		WaitMappedDown,
		WaitMappedDownSpaceEmitted,
		WaitMappedUp,
		WaitMappedUpSpaceEmitted,
		Mapping,
		NumStates,
		Self,
		NA
	}

	public enum Event
	{
		ActivationDown,
		ActivationUp,
		MappedKeyDown,
		MappedKeyUp,
		OtherKeyDown,
		OtherKeyUp,
		ConfigKeyDown,
		NumEvents
	}

	public enum Action
	{
		DiscardKey,
		TapActivationKey,
		DelayKeyDown,
		MapKeyUp,
		MapKeyDown,
		ActivationKeyDownThenKey,
		EmitActDownSavedDownActUp,
		MapSavedAndMapCurrentDown,
		MapSavedAndMapCurrentUp,
		EmitActSavedAndCurrentDown,
		EmitActSavedAndCurrentUp,
		EmitSavedDownAndActUp,
		EmitSavedAndCurrentDown,
		DiscardKeyAndReleaseMappedKeys,
		RunConfigure,
		Null
	}

	public class KeyCodeEvent : EventArgs
	{
		public vkCode code;
		public bool up;
	}

	public class StateMachine
	{
        public event EventHandler<KeyCodeEvent> SendInput;
        public event EventHandler Deactivate;

		protected virtual void OnSendInput(KeyCodeEvent e) => SendInput?.Invoke(this, e);
		protected virtual void OnDeactivate() => Deactivate?.Invoke(this, new EventArgs());

		private class StartState
		{
			readonly State CurrentState;
			readonly Event Command;

			public StartState(State currentState, Event command)
			{
				CurrentState = currentState;
				Command = command;
			}

			public override int GetHashCode()
			{
				return 17 * 31 * CurrentState.GetHashCode() + 31 * Command.GetHashCode();
			}

			public override bool Equals(object obj)
			{
				StartState other = obj as StartState;
				return other != null && CurrentState == other.CurrentState && Command == other.Command;
			}
		}

		private class EndState
		{
			public readonly State NewState;
			public readonly Action Action;

			public EndState(State newState, Action action)
			{
				NewState = newState;
				Action = action;
			}

			public override int GetHashCode()
			{
				return 17 * 31 * NewState.GetHashCode() + 31 * NewState.GetHashCode();
			}

			public override bool Equals(object obj)
			{
				EndState other = obj as EndState;
				return other != null && NewState == other.NewState && Action == other.Action;
			}
		}

		Dictionary<StartState, EndState> transitions;
		public State CurrentState { get; private set; }
		public Action CurrentAction { get; private set; }

		private List<KeyToKey> _mappings;

		public vkCode ActivationKey;
		private vkCode _savedKeyDown = vkCode.VK_NULL;
		private readonly List<vkCode> _mappedKeysHeld = new List<vkCode>();

		public StateMachine(vkCode activationKey, List<KeyToKey> mappings)
		{
			ActivationKey = activationKey;
			_mappings = mappings;

			CurrentState = State.Idle;
			CurrentAction = Action.Null;
			transitions = new Dictionary<StartState, EndState>
			{
				// Idle
				{ new StartState(State.Idle, Event.ActivationDown), new EndState(State.WaitMappedDown, Action.DiscardKey) },
				{ new StartState(State.Idle, Event.ActivationUp),   new EndState(State.Self, Action.Null) },
				{ new StartState(State.Idle, Event.MappedKeyDown),  new EndState(State.Self, Action.Null) },
				{ new StartState(State.Idle, Event.MappedKeyUp),    new EndState(State.Self, Action.Null) },
				{ new StartState(State.Idle, Event.OtherKeyDown),   new EndState(State.Self, Action.Null) },
				{ new StartState(State.Idle, Event.OtherKeyUp),     new EndState(State.Self, Action.Null) },
				{ new StartState(State.Idle, Event.ConfigKeyDown),  new EndState(State.Self, Action.Null) },
				// WaitMappedDown
				{ new StartState(State.WaitMappedDown, Event.ActivationDown), new EndState(State.Self, Action.DiscardKey) },
				{ new StartState(State.WaitMappedDown, Event.ActivationUp),   new EndState(State.Idle, Action.TapActivationKey) },
				{ new StartState(State.WaitMappedDown, Event.MappedKeyDown),  new EndState(State.WaitMappedUp, Action.DelayKeyDown) },
				{ new StartState(State.WaitMappedDown, Event.MappedKeyUp),    new EndState(State.Self, Action.Null) },
				{ new StartState(State.WaitMappedDown, Event.OtherKeyDown),   new EndState(State.WaitMappedDownSpaceEmitted, Action.ActivationKeyDownThenKey) },
				{ new StartState(State.WaitMappedDown, Event.OtherKeyUp),     new EndState(State.Self, Action.Null) },
				// TODO: We don't have this configure stuff
				{ new StartState(State.WaitMappedDown, Event.ConfigKeyDown),  new EndState(State.Self, Action.RunConfigure) },
				// WaitMappedDownSpaceEmitted
				{ new StartState(State.WaitMappedDownSpaceEmitted, Event.ActivationDown), new EndState(State.Self, Action.DiscardKey) },
				{ new StartState(State.WaitMappedDownSpaceEmitted, Event.ActivationUp),   new EndState(State.Idle, Action.Null) },
				{ new StartState(State.WaitMappedDownSpaceEmitted, Event.MappedKeyDown),  new EndState(State.WaitMappedUpSpaceEmitted, Action.DelayKeyDown) },
				{ new StartState(State.WaitMappedDownSpaceEmitted, Event.MappedKeyUp),    new EndState(State.Self, Action.Null) },
				{ new StartState(State.WaitMappedDownSpaceEmitted, Event.OtherKeyDown),   new EndState(State.Self, Action.Null) },
				{ new StartState(State.WaitMappedDownSpaceEmitted, Event.OtherKeyUp),     new EndState(State.Self, Action.Null) },
				{ new StartState(State.WaitMappedDownSpaceEmitted, Event.ConfigKeyDown),  new EndState(State.Self, Action.RunConfigure) },
				// WaitMappedUp (still might emit the activation key)
				{ new StartState(State.WaitMappedUp, Event.ActivationDown), new EndState(State.Self, Action.DiscardKey) },
				{ new StartState(State.WaitMappedUp, Event.ActivationUp),   new EndState(State.Idle, Action.EmitActDownSavedDownActUp) },
				{ new StartState(State.WaitMappedUp, Event.MappedKeyDown),  new EndState(State.Mapping, Action.MapSavedAndMapCurrentDown) },
				{ new StartState(State.WaitMappedUp, Event.MappedKeyUp),    new EndState(State.Mapping, Action.MapSavedAndMapCurrentUp) },
				{ new StartState(State.WaitMappedUp, Event.OtherKeyDown),   new EndState(State.Idle, Action.EmitActSavedAndCurrentDown) },
				{ new StartState(State.WaitMappedUp, Event.OtherKeyUp),     new EndState(State.WaitMappedUpSpaceEmitted, Action.EmitActSavedAndCurrentUp) },
				{ new StartState(State.WaitMappedUp, Event.ConfigKeyDown),  new EndState(State.Self, Action.RunConfigure) },
				// WaitMappedUpSpaceEmitted
				{ new StartState(State.WaitMappedUpSpaceEmitted, Event.ActivationDown), new EndState(State.Self, Action.DiscardKey) },
				{ new StartState(State.WaitMappedUpSpaceEmitted, Event.ActivationUp),   new EndState(State.Idle, Action.EmitSavedDownAndActUp) },
				{ new StartState(State.WaitMappedUpSpaceEmitted, Event.MappedKeyDown),  new EndState(State.Mapping, Action.MapSavedAndMapCurrentDown) },
				{ new StartState(State.WaitMappedUpSpaceEmitted, Event.MappedKeyUp),    new EndState(State.Mapping, Action.MapSavedAndMapCurrentUp) },
				{ new StartState(State.WaitMappedUpSpaceEmitted, Event.OtherKeyDown),   new EndState(State.Idle, Action.EmitSavedAndCurrentDown) },
				{ new StartState(State.WaitMappedUpSpaceEmitted, Event.OtherKeyUp),     new EndState(State.Self, Action.Null) },
				{ new StartState(State.WaitMappedUpSpaceEmitted, Event.ConfigKeyDown),  new EndState(State.Self, Action.RunConfigure) },
				// Mapping (definitely eaten the activation key)
				{ new StartState(State.Mapping, Event.ActivationDown), new EndState(State.Self, Action.DiscardKey) },
				{ new StartState(State.Mapping, Event.ActivationUp),   new EndState(State.Idle, Action.DiscardKeyAndReleaseMappedKeys) },
				{ new StartState(State.Mapping, Event.MappedKeyDown),  new EndState(State.Self, Action.MapKeyDown) },
				{ new StartState(State.Mapping, Event.MappedKeyUp),    new EndState(State.Self, Action.MapKeyUp) },
				{ new StartState(State.Mapping, Event.OtherKeyDown),   new EndState(State.Self, Action.Null) },
				{ new StartState(State.Mapping, Event.OtherKeyUp),     new EndState(State.Self, Action.Null) },
				{ new StartState(State.Mapping, Event.ConfigKeyDown),  new EndState(State.Self, Action.RunConfigure) },
			};
		}

		private EndState GetNext(Event command)
		{
			StartState transition = new StartState(CurrentState, command);
			EndState endState;
			if (!transitions.TryGetValue(transition, out endState))
				throw new Exception("Invalid transition: " + CurrentState + " -> " + command);
			return endState;
		}

		private void MoveNext(Event command)
		{
			EndState endState = GetNext(command);
			System.Diagnostics.Debug.WriteLine(endState.NewState);
			if (endState.NewState != State.Self)
            {
				CurrentState = endState.NewState;
            }
			CurrentAction = endState.Action;
		}

		public bool ProcessKey(bool isKeyDown, vkCode code)
		{
			Event e;

			if (code == ActivationKey)
			{
				System.Diagnostics.Debug.WriteLine("activation key");
				e = isKeyDown ? Event.ActivationDown : Event.ActivationUp;
			}
			else if (TranslateCode(code) != vkCode.VK_NULL)
			{
				System.Diagnostics.Debug.WriteLine("mapped key");
				e = isKeyDown ? Event.MappedKeyDown : Event.MappedKeyUp;
			}
			else
			{
				System.Diagnostics.Debug.WriteLine("other key");
				e = isKeyDown ? Event.OtherKeyDown : Event.OtherKeyUp;
			}

			return ProcessEvent(e, code);
		}

		private bool ProcessEvent(Event e, vkCode code)
		{
			// if (GetNext(e).NewState != State.Self) MoveNext(e);
			MoveNext(e);
			if (CurrentState == State.Idle)
            {
				System.Diagnostics.Debug.WriteLine("Deactivated");
				OnDeactivate();
            }

			switch (CurrentAction)
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

		private bool MapKey(vkCode code, bool up)
		{
			System.Diagnostics.Debug.WriteLine("MapKey");
			vkCode newCode = TranslateCode(code);

			if (newCode != vkCode.VK_NULL)
			{
				if (!up) _mappedKeysHeld.Add(code);
				KeyEvent(newCode, up);
				return true;
			}
			return false;
		}

		private bool DiscardKey() => true;

		private bool DiscardKeyAndReleaseMappedKeys(vkCode code)
		{
			System.Diagnostics.Debug.WriteLine("DiscardKeyAndReleaseMappedKeys");
			_mappedKeysHeld.ForEach(k =>
			{
				KeyEvent(code, true);
			});
			_mappedKeysHeld.Clear();
			return true;
		}

		private bool TapActivationKey(vkCode code)
		{
			System.Diagnostics.Debug.WriteLine("TapActivationKey");
			TapKey(ActivationKey);
			return true;
		}

		private bool ActivationKeyDownThenKey(vkCode code)
		{
			System.Diagnostics.Debug.WriteLine("ActivationKeyDownThenKey");
			KeyDown(ActivationKey);
			KeyDown(code);
			return true;
		}

		private bool MapKeyDown(vkCode code)
		{
			System.Diagnostics.Debug.WriteLine("MapKeyDown");
			return MapKey(code, false);
		}

		private bool MapKeyUp(vkCode code)
		{
			System.Diagnostics.Debug.WriteLine("MapKeyUp");
			return MapKey(code, true);
		}

		private bool DelayKeyDown(vkCode code)
		{
			System.Diagnostics.Debug.WriteLine("DelayKeyDown");
			// touchcursor has some assert logic here to make sure
			// the program doesn't get stuck in a bad state
			// but I think it will be unneeded here

			_savedKeyDown = code;
			return true;
		}

		private bool EmitSaved()
		{
			System.Diagnostics.Debug.WriteLine("EmitSaved");
			if (_savedKeyDown != vkCode.VK_NULL)
			{
				KeyDown(_savedKeyDown);
				_savedKeyDown = vkCode.VK_NULL;
			}
			return true;
		}

		private bool EmitActDownSavedDownActUp(vkCode code)
		{
			System.Diagnostics.Debug.WriteLine("EmitActDownSavedDownActUp");
			KeyDown(ActivationKey);
			EmitSaved();
			KeyUp(ActivationKey);
			return true;
		}

		private bool EmitSavedDownAndActUp(vkCode code)
		{
			System.Diagnostics.Debug.WriteLine("EmitSavedDownAndActUp");
			EmitSaved();
			KeyUp(ActivationKey);
			return true;
		}

		private void MapSavedDown()
		{
			System.Diagnostics.Debug.WriteLine("MapSavedDown");
			if (_savedKeyDown != vkCode.VK_NULL)
			{
				MapKeyDown(_savedKeyDown);
				_savedKeyDown = vkCode.VK_NULL;
			}
		}

		private bool MapSavedAndMapCurrentDown(vkCode code)
		{
			System.Diagnostics.Debug.WriteLine("MapSavedAndMapCurrentDown");
			MapSavedDown();
			MapKeyDown(code);
			return true;
		}

		private bool MapSavedAndMapCurrentUp(vkCode code)
		{
			System.Diagnostics.Debug.WriteLine("MapSavedAndMapCurrentUp");
			MapSavedDown();
			MapKeyUp(code);
			return true;
		}

		private bool EmitActSavedAndCurrentDown(vkCode code)
		{
			System.Diagnostics.Debug.WriteLine("EmitActSavedAndCurrentDown");
			KeyDown(ActivationKey);
			EmitSaved();
			KeyDown(code);
			return true;
		}

		private bool EmitActSavedAndCurrentUp(vkCode code)
		{
			System.Diagnostics.Debug.WriteLine("EmitActSavedAndCurrentUp");
			KeyDown(ActivationKey);
			EmitSaved();
			KeyUp(code);
			return true;
		}

		private bool EmitSavedAndCurrentDown(vkCode code)
		{
			System.Diagnostics.Debug.WriteLine("EmitSavedAndCurrentDown");
			EmitSaved();
			KeyDown(code);
			return true;
		}

		private vkCode TranslateCode(vkCode code)
		{
			// TODO: Separate L and R mods
			// TODO: Keys that allow extra mods or not
			bool shift = (PInvoke.GetAsyncKeyState((int)vkCode.VK_CONTROL) & 0xFF00) != 0;
			bool ctrl = (PInvoke.GetAsyncKeyState((int)vkCode.VK_CONTROL) & 0xFF00) != 0;
			bool alt = (PInvoke.GetAsyncKeyState((int)vkCode.VK_MENU) & 0xFF00) != 0;
			bool win = (PInvoke.GetAsyncKeyState((int)vkCode.VK_LWIN) & 0xFF00) != 0 ||
				       (PInvoke.GetAsyncKeyState((int)vkCode.VK_RWIN) & 0xFF00) != 0;

			KeyToKey map = _mappings.Where(x => x.From.Code == code)
				.Where(x =>
                    x.From.Mods.Contains(vkCode.VK_SHIFT) == shift &&
                    x.From.Mods.Contains(vkCode.VK_CONTROL) == ctrl &&
                    x.From.Mods.Contains(vkCode.VK_MENU) == alt &&
                    x.From.Mods.Contains(vkCode.VK_WIN) == win
                ).FirstOrDefault();

			if (map == null) return vkCode.VK_NULL;
			return map.To.Code;
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
				// This is supposed to be mods not mapped keys
 				// _mappedKeysHeld.ForEach(k =>
				// {
				// 	OnSendInput(new KeyCodeEvent { code = k, up = false });
				// });
			}
			OnSendInput(new KeyCodeEvent { code = code, up = up });
			if (!up)
			{
				// This is supposed to be mods not mapped keys
				// _mappedKeysHeld.ForEach(k =>
				// {
				// 	OnSendInput(new KeyCodeEvent { code = k, up = true });
				// });
			}
		}

	}
}
