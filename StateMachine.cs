using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

	public class StateMachine
	{
		public event EventHandler DiscardKey;
		public event EventHandler TapActivationKey;
		public event EventHandler DelayKeyDown;
		public event EventHandler MapKeyUp;
		public event EventHandler MapKeyDown;
		public event EventHandler ActivationKeyDownThenKey;
		public event EventHandler EmitActDownSavedDownActUp;
		public event EventHandler MapSavedAndMapCurrentDown;
		public event EventHandler MapSavedAndMapCurrentUp;
		public event EventHandler EmitActSavedAndCurrentDown;
		public event EventHandler EmitActSavedAndCurrentUp;
		public event EventHandler EmitSavedDownAndActUp;
		public event EventHandler EmitSavedAndCurrentDown;
		public event EventHandler DiscardKeyAndReleaseMappedKeys;
		// TODO: Figure out how to null this
		public event EventHandler RunConfigure;

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

		public class EndState
		{
			public readonly State NewState;
			public readonly EventHandler Action;

			public EndState(State newState, EventHandler action)
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

		public StateMachine()
		{
			CurrentState = State.Idle;
			transitions = new Dictionary<StartState, EndState>
			{
				// Idle
				{ new StartState(State.Idle, Event.ActivationDown), new EndState(State.WaitMappedDown, DiscardKey) },
				{ new StartState(State.Idle, Event.ActivationUp),   new EndState(State.Self, null) },
				{ new StartState(State.Idle, Event.MappedKeyDown),  new EndState(State.Self, null) },
				{ new StartState(State.Idle, Event.MappedKeyUp),    new EndState(State.Self, null) },
				{ new StartState(State.Idle, Event.OtherKeyDown),   new EndState(State.Self, null) },
				{ new StartState(State.Idle, Event.OtherKeyUp),     new EndState(State.Self, null) },
				{ new StartState(State.Idle, Event.ConfigKeyDown),  new EndState(State.Self, null) },
				// WaitMappedDown
				{ new StartState(State.WaitMappedDown, Event.ActivationDown), new EndState(State.Self, DiscardKey) },
				{ new StartState(State.WaitMappedDown, Event.ActivationUp),   new EndState(State.Idle, TapActivationKey) },
				{ new StartState(State.WaitMappedDown, Event.MappedKeyDown),  new EndState(State.WaitMappedUp, DelayKeyDown) },
				{ new StartState(State.WaitMappedDown, Event.MappedKeyUp),    new EndState(State.Self, null) },
				{ new StartState(State.WaitMappedDown, Event.OtherKeyDown),   new EndState(State.WaitMappedDownSpaceEmitted, ActivationKeyDownThenKey) },
				{ new StartState(State.WaitMappedDown, Event.OtherKeyUp),     new EndState(State.Self, null) },
				// TODO: We don't have this configure stuff
				{ new StartState(State.WaitMappedDown, Event.ConfigKeyDown),  new EndState(State.Self, RunConfigure) },
				// WaitMappedDownSpaceEmitted
				{ new StartState(State.WaitMappedDownSpaceEmitted, Event.ActivationDown), new EndState(State.Self, DiscardKey) },
				{ new StartState(State.WaitMappedDownSpaceEmitted, Event.ActivationUp),   new EndState(State.Idle, null) },
				{ new StartState(State.WaitMappedDownSpaceEmitted, Event.MappedKeyDown),  new EndState(State.WaitMappedUpSpaceEmitted, DelayKeyDown) },
				{ new StartState(State.WaitMappedDownSpaceEmitted, Event.MappedKeyUp),    new EndState(State.Self, null) },
				{ new StartState(State.WaitMappedDownSpaceEmitted, Event.OtherKeyDown),   new EndState(State.Self, null) },
				{ new StartState(State.WaitMappedDownSpaceEmitted, Event.OtherKeyUp),     new EndState(State.Self, null) },
				{ new StartState(State.WaitMappedDownSpaceEmitted, Event.ConfigKeyDown),  new EndState(State.Self, RunConfigure) },
				// WaitMappedUp (still might emit the activation key)
				{ new StartState(State.WaitMappedUp, Event.ActivationDown), new EndState(State.Self, DiscardKey) },
				{ new StartState(State.WaitMappedUp, Event.ActivationUp),   new EndState(State.Idle, EmitActDownSavedDownActUp) },
				{ new StartState(State.WaitMappedUp, Event.MappedKeyDown),  new EndState(State.Mapping, MapSavedAndMapCurrentDown) },
				{ new StartState(State.WaitMappedUp, Event.MappedKeyUp),    new EndState(State.Mapping, MapSavedAndMapCurrentUp) },
				{ new StartState(State.WaitMappedUp, Event.OtherKeyDown),   new EndState(State.Idle, EmitActSavedAndCurrentDown) },
				{ new StartState(State.WaitMappedUp, Event.OtherKeyUp),     new EndState(State.WaitMappedUpSpaceEmitted, EmitActSavedAndCurrentUp) },
				{ new StartState(State.WaitMappedUp, Event.ConfigKeyDown),  new EndState(State.Self, RunConfigure) },
				// WaitMappedUpSpaceEmitted
				{ new StartState(State.WaitMappedUpSpaceEmitted, Event.ActivationDown), new EndState(State.Self, DiscardKey) },
				{ new StartState(State.WaitMappedUpSpaceEmitted, Event.ActivationUp),   new EndState(State.Idle, EmitSavedDownAndActUp) },
				{ new StartState(State.WaitMappedUpSpaceEmitted, Event.MappedKeyDown),  new EndState(State.Mapping, MapSavedAndMapCurrentDown) },
				{ new StartState(State.WaitMappedUpSpaceEmitted, Event.MappedKeyUp),    new EndState(State.Mapping, MapSavedAndMapCurrentUp) },
				{ new StartState(State.WaitMappedUpSpaceEmitted, Event.OtherKeyDown),   new EndState(State.Idle, EmitSavedAndCurrentDown) },
				{ new StartState(State.WaitMappedUpSpaceEmitted, Event.OtherKeyUp),     new EndState(State.Self, null) },
				{ new StartState(State.WaitMappedUpSpaceEmitted, Event.ConfigKeyDown),  new EndState(State.Self, RunConfigure) },
				// Mapping (definitely eaten the activation key)
				{ new StartState(State.Mapping, Event.ActivationDown), new EndState(State.Self, DiscardKey) },
				{ new StartState(State.Mapping, Event.ActivationUp),   new EndState(State.Idle, DiscardKeyAndReleaseMappedKeys) },
				{ new StartState(State.Mapping, Event.MappedKeyDown),  new EndState(State.Mapping, MapKeyDown) },
				{ new StartState(State.Mapping, Event.MappedKeyUp),    new EndState(State.Mapping, MapKeyUp) },
				{ new StartState(State.Mapping, Event.OtherKeyDown),   new EndState(State.Idle, null) },
				{ new StartState(State.Mapping, Event.OtherKeyUp),     new EndState(State.Self, null) },
				{ new StartState(State.Mapping, Event.ConfigKeyDown),  new EndState(State.Self, RunConfigure) },
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

		public void MoveNext(Event command)
		{
			EndState endState = GetNext(command);
			CurrentState = endState.NewState;
			endState.Action?.Invoke(this, new EventArgs());
		}
	}
}
