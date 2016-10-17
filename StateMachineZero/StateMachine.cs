using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace FunctionZero.StateMachineZero
{
	public class StateMachine<TStates, TMessages> : INotifyPropertyChanged
	{
		/// <summary>
		/// If this  changes, an event is raised. You can bind UI to this property.
		/// There is also a StateChanged event that carries more information.
		/// This raises a propertyChanged event immediately BEFORE the StateChanged event.
		/// </summary>
		public TStates State
		{
			get { return _state; }
			private set
			{
				if(_state.ToString() != value.ToString())
				{
					_state = value;
					OnPropertyChanged();
				}
			}
		}

		public string Name { get; }

		/// <summary>
		/// A user object that can be accessed indirectly via the PropertyChanged or StateChanged event.
		/// </summary>
		public object StateObject { get; }

		public delegate StateChangingEventArgs<TStates> GetStateChangeEventArgsDelegate(TMessages message, object messagePayload);

		private Dictionary<StateTransition<TStates, TMessages>, GetStateChangeEventArgsDelegate> StateTransitionList { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="startingState">The initial state for the state machine.</param>
		/// <param name="name">Optional name for the state machine.</param>
		/// <param name="stateObject">A user object that can be accessed indirectly via the PropertyChanged or StateChanged event.</param>
		public StateMachine(TStates startingState, string name = "Unnamed", object stateObject = null)
		{
			// TODO: Find a way to do this for Universal libraries.
			//if(!typeof(TStates).IsEnum)
			//	throw new ArgumentException("TStates must be an enumerated type");
			//if(!typeof(TMessages).IsEnum)
			//	throw new ArgumentException("TMessages must be an enumerated type");

			_messageQueue = new Queue<Tuple<TMessages, object>>();
			_messageStack = new Stack<Tuple<TMessages, object>>();

			StateTransitionList = new Dictionary<StateTransition<TStates, TMessages>, GetStateChangeEventArgsDelegate>();
			State = startingState;
			Name = name;
			StateObject = stateObject;
		}

		public int PushMessage(TMessages message, object payload = null)
		{
			_messageStack.Push(new Tuple<TMessages, object>(message, payload));
			return _messageStack.Count;
		}

		public int PopMessage()
		{
			var popped = _messageStack.Pop();
			this.EnqueueMessage(popped.Item1, popped.Item2);
			return _messageStack.Count;
		}

		/// <summary>
		/// This method creates a new StateTransition to the  StateMachine.
		/// </summary>
		/// <param name="currentState">This is the state the machine must be in for this transition to apply.</param>
		/// <param name="message">This is the message that is given to the machine</param>
		/// <param name="getNextState">This is a method that accepts a message and a message-payload and returns a new state with a resultant payload.
		/// The input message and payload for this delegate are sourced from the EnqueueMessage call (i.e. when a message is queued)</param>
		public void Add(TStates currentState, TMessages message, GetStateChangeEventArgsDelegate getNextState)
		{
			this.StateTransitionList.Add(new StateTransition<TStates, TMessages>(currentState, message), getNextState);
		}

		/// <summary>
		/// This method creates a new StateTransition to the StateMachine.
		/// </summary>
		/// <param name="currentState">This is the state the machine must be in for this transition to apply.</param>
		/// <param name="message">This is the message that is given to the machine</param>
		/// <param name="nextState">This is the state to change to when the message is processed.</param>
		public void Add(TStates currentState, TMessages message, TStates nextState)
		{
			this.Add(currentState, message, (messages, payload) => new StateChangingEventArgs<TStates>(nextState, payload));
		}

		private StateChangingEventArgs<TStates> GetNextState(TMessages message, object messagePayload)
		{
			StateTransition<TStates, TMessages> transition = new StateTransition<TStates, TMessages>(State, message);
			GetStateChangeEventArgsDelegate nextStateGetter;

			if(!StateTransitionList.TryGetValue(transition, out nextStateGetter))
			{
				//Debug.Assert(false, "State transition problem.");

				var temp = UnknownStateTransition;

				if(temp != null)
				{
					temp.Invoke(this, new UnknownStateTransitionEventArgs<TStates, TMessages>(State, message, messagePayload));
				}
				else
				{
					throw new Exception("Invalid transition: " + State + " -> " + message + " on machine '" + this.Name + "'");
				}
				return null;
			}

			return nextStateGetter(message, messagePayload);
		}

		public event EventHandler<StateChangedEventArgs<TStates>> StateChanged;

		public event EventHandler<UnknownStateTransitionEventArgs<TStates, TMessages>> UnknownStateTransition;

		protected virtual void OnNotifyStateChanged(StateChangedEventArgs<TStates> e)
		{
			StateChanged?.Invoke(this, e);
		}

		private bool _reentrancyGuard = false;

		private TStates ProcessMessage(TMessages message, object messagePayload)
		{
			StateChangingEventArgs<TStates> temp = GetNextState(message, messagePayload);

			if(temp != null)
			{
				if(_reentrancyGuard == true)
				{
					throw new Exception("Broken");
				}
				else
				{
					_reentrancyGuard = true;

					TStates oldState = State;
					State = temp.NewState; // This is where the state is changed.	

					// This is done here rather than in the OnPropertyChanged for State because the OnNotifyStateChangedEventArgs requires oldState and payLoad.
					if(State.ToString() != oldState.ToString())
					{
						StateChangedEventArgs<TStates> retVal = new StateChangedEventArgs<TStates>(temp.NewState, oldState, temp.PayLoad);
						OnNotifyStateChanged(retVal);
					}
					_reentrancyGuard = false;
				}
			}
			return State;
		}

		private readonly Queue<Tuple<TMessages, object>> _messageQueue;
		private readonly Stack<Tuple<TMessages, object>> _messageStack;
		private TStates _state;

		/// <summary>
		///     This is not threadsafe.
		///     Sending a message to a this can cause it to change state, which in turn raises a StateChanged event.
		///     This method puts a message into the message queue. If the queue is empty, the message gets processed immediately.
		///     Processing the message may cause further messages to be queued up, and when the original message is complete
		///     those further messages are processed. They in turn may queue up further messages and so on.
		///     The queue is used so that a message from a StateChanged subscriber cannot cause a state change while the StateChanged event is still notifying other subscribers.
		///     More broadly speaking, it prevents a message from being processed while a previous message is still being processed.
		/// </summary>
		/// <param name="message">The message to be queued</param>
		/// <param name="description">Optional parameter can be used by the message consumer.</param>
		/// <param name="payLoad">Optional parameter may be used by the message consumer.</param>
		/// <param name="messagePayload"></param>
		/// <returns></returns>
		public int EnqueueMessage(TMessages message, object messagePayload = null)
		{
			if(_messageQueue.Count == 0)
			{
				_messageQueue.Enqueue(new Tuple<TMessages, object>(message, messagePayload));
				bool debugFlag = true;
				while(_messageQueue.Count != 0)
				{
					Tuple<TMessages, object> nextMessage = _messageQueue.Peek();

					if(debugFlag == true)
					{
						Debug.WriteLine("Starting immediate message on       " + this.Name + "  : " + nextMessage.ToString());
						debugFlag = false;
					}
					else
					{
						Debug.WriteLine("Starting dequeued message on        " + this.Name + "  : " + nextMessage.ToString());
					}

					ProcessMessage(nextMessage.Item1, nextMessage.Item2); // This might cause more messages to be queued for processing AFTER THIS MESSAGE HAS COMPLETED!
					Debug.WriteLine("Ending message on                   " + this.Name + "  : " + nextMessage.ToString());
					_messageQueue.Dequeue();
				}
			}
			else
			{
				Debug.WriteLine("Enqueueing message on               " + this.Name + "  : " + message.ToString());
				_messageQueue.Enqueue(new Tuple<TMessages, object>(message, messagePayload));
			}

			int retval = _messageQueue.Count;

			return retval;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public void AddStateEnterEvent(TStates state, Action<object> action, object o)
		{
			throw new NotImplementedException();
		}

		public void AddStateLeaveEvent(TStates state, Action<object> action, object o)
		{
			throw new NotImplementedException();
		}
	}
}