using System;
using System.Collections.Generic;

namespace FunctionZero.StateMachineZero
{
	public class MessageQueue
	{
		private Queue<Action> _messageQueue;

		public MessageQueue()
		{
			_messageQueue = new Queue<Action>();
		}

		public int Count => _messageQueue.Count;

		public void PostMessage(Action message)
		{
			_messageQueue.Enqueue(message);

			// If the queue just woke up from empty, start pumping it.
			if(_messageQueue.Count == 1)
			{
				// Pump the queue until empty.
				while (_messageQueue.Count != 0)
				{
					// Use Peek because we don't want to dequeue a message until it has been processed.
					// Reason: If the last message in the queue posts a new message to an empty queue, the while-loop
					// will start spinning in a recursive call.
					var currentMessage = _messageQueue.Peek();
					currentMessage();
					_messageQueue.Dequeue();
				}
			}
		}
	}
}
