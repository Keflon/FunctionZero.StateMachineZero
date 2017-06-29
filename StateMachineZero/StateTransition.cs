namespace FunctionZero.StateMachineZero
{
	public class StateTransition<TStates, TMessages>
	{
		TStates State { get; }
		TMessages Message { get; }

		public StateTransition(TStates state, TMessages message)
		{
			State = state;
			Message = message;
		}

		public override int GetHashCode()
		{
			return 17 + 31 * State.GetHashCode() + 31 * Message.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			StateTransition<TStates, TMessages> other = obj as StateTransition<TStates, TMessages>;
			return other != null && State.Equals(other.State) && Message.Equals(other.Message);
		}
	}
}