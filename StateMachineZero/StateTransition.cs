namespace FunctionZero.StateMachineZero
{
	public class StateTransition<TStates, TMessages>
	{
		TStates CurrentState { get; }
		TMessages Message { get; }

		public StateTransition(TStates currentState, TMessages message)
		{
			CurrentState = currentState;
			Message = message;
		}

		public override int GetHashCode()
		{
			return 17 + 31 * CurrentState.GetHashCode() + 31 * Message.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			StateTransition<TStates, TMessages> other = obj as StateTransition<TStates, TMessages>;
			//return other != null && CurrentState == other.CurrentState && this.Message == other.Message;
			return other != null && CurrentState.ToString() == other.CurrentState.ToString() && this.Message.ToString() == other.Message.ToString();
			//return other != null && CurrentState.GetHashCode() == other.GetHashCode() && this.Message.GetHashCode() == other.Message.GetHashCode();
		}
	}
}