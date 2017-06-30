using System;

namespace FunctionZero.StateMachineZero
{
	public enum StateChangeMode
	{
		Changing = 0,
		Changed
	}
	public class StateChangeEventArgs<TState, TPayload> : EventArgs
	{
		public TState NewState { get; }
		public TState OldState { get; }
		public StateChangeMode ChangeType { get; }
		public TPayload PayLoad { get; }
		public bool StateFaultOccured { get; }

		public StateChangeEventArgs(TState newState, TState oldState, StateChangeMode changeType, TPayload payLoad, bool stateFaultOccured)
		{
			// TODO: Find a way to confirm TState is an enum.
			NewState = newState;
			OldState = oldState;
			ChangeType = changeType;
			PayLoad = payLoad;
			StateFaultOccured = stateFaultOccured;
		}
	}
}
