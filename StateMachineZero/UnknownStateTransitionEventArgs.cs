using System;

namespace FunctionZero.StateMachineZero
{
	public class UnknownStateTransitionEventArgs<TState, TMessage, TPayload> : EventArgs
	{
		public TState CurrentState { get; }
		public TMessage Message { get; }
		public TPayload PayLoad { get; }

		public UnknownStateTransitionEventArgs(TState currentState, TMessage message, TPayload payload)
		{
			CurrentState = currentState;
			Message = message;
			PayLoad = payload;
		}
	}
}
