using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace FunctionZero.StateMachineZero
{
    public class StateMachine<TState, TMessage, TPayload>
        : INotifyPropertyChanged
        where TState : Enum where TMessage : Enum
    {
        private readonly MessageQueue _messageQueue;
        private readonly Dictionary<TState, StateAction> _allStatesEntered;
        private readonly Dictionary<TState, StateAction> _allStatesLeft;

        private readonly Dictionary<TState, StateAction> _allStatesEntering;
        private readonly Dictionary<TState, StateAction> _allStatesLeaving;

        private TState _state;

        public event EventHandler<StateChangeEventArgs<TState, TMessage, TPayload>> StateChanging;
        public event EventHandler<StateChangeEventArgs<TState, TMessage, TPayload>> StateChanged;
        //private event EventHandler<StateChangeEventArgs<TState, TPayload>> StateEntered;


        //private event EventHandler<StateChangeEventArgs<TState, TPayload>> StateLeft;
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
                if (!_state.Equals(value))
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
        public delegate void StateAction(TState from, TState to, TPayload payload);
        private readonly Dictionary<StateTransition<TState, TMessage>, GetStateDelegate> _stateTransitions;

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

            _stateTransitions = new Dictionary<StateTransition<TState, TMessage>, GetStateDelegate>();
            _allStatesEntered = new Dictionary<TState, StateAction>();
            _allStatesLeft = new Dictionary<TState, StateAction>();
            _allStatesEntering = new Dictionary<TState, StateAction>();
            _allStatesLeaving = new Dictionary<TState, StateAction>();
            Name = name;
            StateObject = stateObject;
            State = startingState;

            Array values = Enum.GetValues(typeof(TState));

            foreach (TState val in values)
            {
                _allStatesEntered[val] = (fromState, toState, payload) => { };
                _allStatesLeft[val] = (fromState, toState, payload) => { };
                _allStatesEntering[val] = (fromState, toState, payload) => { };
                _allStatesLeaving[val] = (fromState, toState, payload) => { };
            }
        }

        /// <summary>
        /// This method adds a new StateTransition to the  StateMachine.
        /// </summary>
        /// <param name="currentState">This is the state the machine must be in for this transition to apply.</param>
        /// <param name="message">This is the message that is given to the machine</param>
        /// <param name="getNextState">This is a method that accepts a message and returns the state the message transitions to.</param>
        public void Add(TState currentState, TMessage message, GetStateDelegate getNextState/* TODO: transitionFiredEvent*/)
        {
            this._stateTransitions.Add(new StateTransition<TState, TMessage>(currentState, message), getNextState);
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
        //private bool GetNextState(TMessage message, TPayload payload, ref TState state)
        //{
        //    GetStateDelegate nextStateGetter;

        //    if (!_stateTransitions.TryGetValue(new StateTransition<TState, TMessage>(State, message), out nextStateGetter))
        //    {
        //        return false;
        //    }
        //    state = nextStateGetter(message, payload);
        //    return true;
        //}

        protected virtual void OnNotifyStateChanging(StateChangeEventArgs<TState, TMessage, TPayload> e)
        {
            _allStatesLeaving[e.OldState](e.OldState, e.NewState, e.PayLoad);
            _allStatesEntering[e.NewState](e.OldState, e.NewState, e.PayLoad);
            StateChanging?.Invoke(this, e);
        }

        protected virtual void OnNotifyStateChanged(StateChangeEventArgs<TState, TMessage, TPayload> e)
        {
            _allStatesLeft[e.OldState](e.OldState, e.NewState, e.PayLoad);
            _allStatesEntered[e.NewState](e.OldState, e.NewState, e.PayLoad);
            StateChanged?.Invoke(this, e);
        }

        protected virtual void OnNotifyStateFault(BadTransitionEventArgs<TState, TMessage, TPayload> e)
        {
            BadTransition?.Invoke(this, e);
        }

        private bool _reentrancyGuard = false;

        private void ProcessMessage(TMessage message, TPayload messagePayload)
        {
            if (_reentrancyGuard == true)
            {
                throw new Exception("Broken");
            }
            _reentrancyGuard = true;

            TState nextState = default(TState);

            TState oldState = State;
            bool faulted = false;
            //if (GetNextState(message, messagePayload, ref nextState) == false)
            if (!_stateTransitions.TryGetValue(new StateTransition<TState, TMessage>(State, message), out var nextStateGetter))
            {
                var faultEventArgs = new BadTransitionEventArgs<TState, TMessage, TPayload>(State, message, messagePayload);

                OnNotifyStateFault(faultEventArgs);

                if (faultEventArgs.RequestedState == null)
                {
                    _reentrancyGuard = false;
                    return;
                }

                faulted = true;
                nextState = faultEventArgs.RequestedState.State;
                messagePayload = faultEventArgs.RequestedState.Payload;
            }
            else if (nextStateGetter != null)
            {
                nextState = nextStateGetter(message, messagePayload);
            }
            else
            {
                // If a transition has no getter, it just swallows the message.
                _reentrancyGuard = false;
                return;
            }
            // Notify even if state isn't changing / hasn't changed.
            OnNotifyStateChanging(new StateChangeEventArgs<TState, TMessage, TPayload>(nextState, oldState, message, StateChangeMode.Changing, messagePayload, faulted));
            State = nextState; // This is where the state is changed.	
            OnNotifyStateChanged(new StateChangeEventArgs<TState, TMessage, TPayload>(nextState, oldState, message, StateChangeMode.Changed, messagePayload, faulted));

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

        public void SetStateEnterEvent(TState state, StateAction action)
        {
            _allStatesEntered[state] = action;

        }

        public void SetStateLeaveEvent(TState state, StateAction action)
        {
            _allStatesLeft[state] = action;
        }
        public void SetStateEnteringEvent(TState state, StateAction action)
        {
            _allStatesEntering[state] = action;

        }

        public void SetStateLeavingEvent(TState state, StateAction action)
        {
            _allStatesLeaving[state] = action;
        }

        //public void AddTransitionFiredEvent()
        //{
        // This is unnecessary because it would simply replicate what the stateGetter already does.
        //    throw new NotImplementedException();
        //}
    }
}