using FunctionZero.StateMachineZero;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMachineZeroTests
{
    [TestClass]
    public class TrafficLightSample
    {
        // Define your states ...
        enum TrafficLightStates
        {
            None,
            Initialising = 0,
            Red,
            RedAmber,
            Green,
            Amber
        }
        // Define your messages ...
        enum TrafficLightMessages
        {
            Initialize = 0,
            Next,
        }

        [TestMethod]
        public void TestTrafficLightSequence()
        {
            // The state machine needs a queue ...
            var queue = new MessageQueue();

            var machine = new StateMachine<TrafficLightStates, TrafficLightMessages, string>(queue, TrafficLightStates.None, "Traffic Light", "This is a state object");

            // I am a state machine.
            // If I am in the None state and receive an Initialize message, I will transition to the Initializing state ...
            machine.Add(TrafficLightStates.None, TrafficLightMessages.Initialize, TrafficLightStates.Initialising);
        }
    }
}
