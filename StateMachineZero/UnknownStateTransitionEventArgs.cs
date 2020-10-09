using System;

namespace FunctionZero.StateMachineZero
{
	public class BadTransitionEventArgs<TState, TMessage, TPayload> : EventArgs
	{
		public TState CurrentState { get; }
		public TMessage Message { get; }
		public TPayload PayLoad { get; }

		internal StatePayload<TState, TPayload> RequestedState { get; private set; }

		public BadTransitionEventArgs(TState currentState, TMessage message, TPayload payload)
		{
			CurrentState = currentState;
			Message = message;
			PayLoad = payload;
		}

		public void RequestState(TState state, TPayload payload)
		{
			this.RequestedState = new StatePayload<TState, TPayload>(state, payload);
		}
	}

	internal class StatePayload<TState, TPayload>
	{
		public TState State { get; }
		public TPayload Payload { get; }

		public StatePayload(TState state, TPayload payload)
		{
			State = state;
			Payload = payload;
		}
	}
}
