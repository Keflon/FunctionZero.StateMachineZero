using System;

namespace FunctionZero.StateMachineZero
{
	public enum StateChangeMode
	{
		Changing = 0,
		Changed
	}
	public class StateChangeEventArgs<TState, TMessage, TPayload> 
		: EventArgs where TState : Enum where TMessage : Enum
	{
		public TState NewState { get; }
		public TState OldState { get; }
        public TMessage Message { get; }
        public StateChangeMode ChangeType { get; }
		public TPayload PayLoad { get; }
		public bool StateFaultOccured { get; }

		public StateChangeEventArgs(TState newState, TState oldState, TMessage message, StateChangeMode changeType, TPayload payLoad, bool stateFaultOccured)
		{
			// TODO: Find a way to confirm TState is an enum.
			NewState = newState;
			OldState = oldState;
            Message = message;
            ChangeType = changeType;
			PayLoad = payLoad;
			StateFaultOccured = stateFaultOccured;
		}
	}
}
