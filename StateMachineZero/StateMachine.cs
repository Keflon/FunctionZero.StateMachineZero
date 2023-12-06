using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace FunctionZero.StateMachineZero
{
	public class StateMachine<TState, TMessage, TPayload> : INotifyPropertyChanged
	{
		private readonly MessageQueue _messageQueue;
		private TState _state;

		public event EventHandler<StateChangeEventArgs<TState, TPayload>> StateChanging;
		public event EventHandler<StateChangeEventArgs<TState, TPayload>> StateChanged;
		public event EventHandler<BadTransitionEventArgs<TState, TMessage, TPayload>> BadTransition;

		/// <summary>
		/// This raises a propertyChanged event inbetween the StateChanging and StateChanged events.
		/// StateChanging and StateChanged events carry more information.
		/// </summary>
		public TState State
		{
			get => _state;
			private set
			{
				if(!_state.Equals(value))
				{
					_state = value;
					OnPropertyChanged();
				}
			}
		}

		/// <summary>
		/// A friendly name for each machine instance. Useful for debugging.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// A user object that can be accessed indirectly via the PropertyChanged or StateChanged event.
		/// </summary>
		public object StateObject { get; }

		public delegate TState GetStateDelegate(TMessage message, TPayload payload);

		private Dictionary<StateTransition<TState, TMessage>, GetStateDelegate> StateTransitions { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="queue">The queue this state machine is bound to.</param>
		/// <param name="startingState">The initial state for the state machine.</param>
		/// <param name="name">Optional name for the state machine.</param>
		/// <param name="stateObject">A user object that can be accessed indirectly via the PropertyChanged or StateChanged event.</param>
		public StateMachine(MessageQueue queue, TState startingState, string name = "Unnamed", object stateObject = null)
		{
			// TODO: Find a way to confirm TState and TMessages are enums.
			_messageQueue = queue;

			StateTransitions = new Dictionary<StateTransition<TState, TMessage>, GetStateDelegate>();
			Name = name;
			StateObject = stateObject;
			State = startingState;
		}

		/// <summary>
		/// This method adds a new StateTransition to the  StateMachine.
		/// </summary>
		/// <param name="currentState">This is the state the machine must be in for this transition to apply.</param>
		/// <param name="message">This is the message that is given to the machine</param>
		/// <param name="getNextState">This is a method that accepts a message and returns the state the message transitions to.</param>
		public void Add(TState currentState, TMessage message, GetStateDelegate getNextState/* TODO: transitionFiredEvent*/)
		{
			this.StateTransitions.Add(new StateTransition<TState, TMessage>(currentState, message), getNextState);
		}

		/// <summary>
		/// This method adds a new StateTransition to the StateMachine.
		/// </summary>
		/// <param name="currentState">This is the state the machine must be in for this transition to apply.</param>
		/// <param name="message">This is the message that is given to the machine</param>
		/// <param name="nextState">This is the state to change to when the message is processed.</param>
		public void Add(TState currentState, TMessage message, TState nextState/* TODO: transitionFiredEvent*/)
		{
			this.Add(currentState, message, (state, payload) => nextState);
		}

		/// <summary>
		/// Returns the state a given message takes us to.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="state"></param>
		/// <returns></returns>
		private bool GetNextState(TMessage message, TPayload payload, ref TState state)
		{
			GetStateDelegate nextStateGetter;

			if(!StateTransitions.TryGetValue(new StateTransition<TState, TMessage>(State, message), out nextStateGetter))
			{
				return false;
			}
			state = nextStateGetter(message, payload);
			return true;
		}

		protected virtual void OnNotifyStateChanging(StateChangeEventArgs<TState, TPayload> e)
		{
			StateChanging?.Invoke(this, e);
		}

		protected virtual void OnNotifyStateChanged(StateChangeEventArgs<TState, TPayload> e)
		{
			StateChanged?.Invoke(this, e);
		}

		protected virtual void OnNotifyStateFault(BadTransitionEventArgs<TState, TMessage, TPayload> e)
		{
			BadTransition?.Invoke(this, e);
		}

		private bool _reentrancyGuard = false;

		private void ProcessMessage(TMessage message, TPayload messagePayload)
		{
			if(_reentrancyGuard == true)
			{
				throw new Exception("Broken");
			}
			_reentrancyGuard = true;

			TState nextState = default(TState);

			TState oldState = State;
			bool faulted = false;
			if(GetNextState(message, messagePayload, ref nextState) == false)
			{
				var faultEventArgs = new BadTransitionEventArgs<TState, TMessage, TPayload>(State, message, messagePayload);

				OnNotifyStateFault(faultEventArgs);

				if(faultEventArgs.RequestedState == null)
				{
					_reentrancyGuard = false;
					return;
				}

				faulted = true;
				nextState = faultEventArgs.RequestedState.State;
				messagePayload = faultEventArgs.RequestedState.Payload;
			}

			// Notify even if state isn't changing / hasn't changed.
			OnNotifyStateChanging(new StateChangeEventArgs<TState, TPayload>(nextState, oldState, StateChangeMode.Changing, messagePayload, faulted));
			State = nextState; // This is where the state is changed.	
			OnNotifyStateChanged(new StateChangeEventArgs<TState, TPayload>(nextState, oldState, StateChangeMode.Changed, messagePayload, faulted));

			_reentrancyGuard = false;
		}

		/// <summary>
		/// This posts a message and a payload to the message queue associated with this machine.
		/// If the queue is empty, the message will be processed immediately, otherwise it will be queued
		/// until any pre-existing messages are processed.
		/// If processing this message causes further messages to be posted, those messages will be processed
		/// atomically and in order, i.e. a message will not be processed during the processing of an earlier message.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="messagePayload"></param>
		/// <returns></returns>
		public int PostMessage(TMessage message, TPayload messagePayload = default(TPayload))
		{
			_messageQueue.PostMessage(() =>
			{
				ProcessMessage(message, messagePayload);
			});

			return _messageQueue.Count;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public void AddStateEnterEvent(TState state, Action<object> action, object o)
		{
			throw new NotImplementedException();
		}

		public void AddStateLeaveEvent(TState state, Action<object> action, object o)
		{
			throw new NotImplementedException();
		}

		public void AddTransitionFiredEvent()
		{
			throw new NotImplementedException();
		}
	}
}