using System;

namespace FunctionZero.StateMachineZero
{
	public class UnknownStateTransitionEventArgs<TState, TMessage> : EventArgs
	{
		public TState CurrentState { get; }
		public TMessage Message { get; }
		public object PayLoad { get; }

		public UnknownStateTransitionEventArgs(TState currentState, TMessage message, object payLoad)
		{
			CurrentState = currentState;
			Message = message;
			PayLoad = payLoad;
		}
	}
}
