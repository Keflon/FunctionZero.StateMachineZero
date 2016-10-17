using System;

namespace FunctionZero.StateMachineZero
{
	public class StateChangingEventArgs<TState> : EventArgs
	{
		public StateChangingEventArgs(TState newState, object payLoad /*= null*/)
		{
			// TODO: Find a way to do this for Universal libraries.
			//if(!typeof(TState).IsEnum)
			//	throw new ArgumentException("TState must be an enumerated type");
			NewState = newState;
			PayLoad = payLoad;
		}

		public TState NewState { get; }
		public object PayLoad { get; }
	}
}