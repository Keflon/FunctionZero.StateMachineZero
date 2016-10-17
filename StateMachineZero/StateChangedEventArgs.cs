using System;

namespace FunctionZero.StateMachineZero
{
	public class StateChangedEventArgs<TState> : EventArgs
	{
		public StateChangedEventArgs(TState newState, TState oldState, object payLoad = null)
		{
			// TODO: Find a way to do this for Universal libraries.
			//if(!typeof(TState).IsEnum)
			//	throw new ArgumentException("TState must be an enumerated type");
			NewState = newState;
			OldState = oldState;
			PayLoad = payLoad;
		}

		public TState NewState { get; }
		public TState OldState { get; }
		public object PayLoad { get; }
	}
}
