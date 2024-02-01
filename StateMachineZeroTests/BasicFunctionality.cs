using FunctionZero.StateMachineZero;
using System.Text;

namespace StateMachineZeroTests
{
    [TestClass]
    public class BasicFunctionality
    {
        enum States
        {
            A,
            B,
            C,
            D,
            E
        }

        enum Messages
        {
            a,
            b,
            c,
            d,
            e
        }

        [TestMethod]
        public void TestBasicFunctionality()
        {
            MessageQueue q = new MessageQueue();
            var machine = new StateMachine<States, Messages, object>(q, States.A, "");

            machine.Add(States.A, Messages.a, States.B);
            machine.Add(States.B, Messages.b, States.C);
            machine.Add(States.C, Messages.c, States.D);
            machine.Add(States.D, Messages.d, States.E);
            machine.Add(States.E, Messages.e, States.A);

            machine.PostMessage(Messages.a);
            Assert.AreEqual(machine.State, States.B);

            machine.PostMessage(Messages.b);
            Assert.AreEqual(machine.State, States.C);

            machine.PostMessage(Messages.c);
            Assert.AreEqual(machine.State, States.D);

            machine.PostMessage(Messages.d);
            Assert.AreEqual(machine.State, States.E);

            machine.PostMessage(Messages.e);
            Assert.AreEqual(machine.State, States.A);
        }
        [TestMethod]
        public void TestEnteringStateWithoutExitTransition()
        {
            MessageQueue q = new MessageQueue();
            var machine = new StateMachine<States, Messages, object>(q, States.A, "");

            machine.Add(States.A, Messages.a, States.B);
            machine.Add(States.B, Messages.b, States.C);
            machine.Add(States.C, Messages.c, States.D);
            machine.Add(States.D, Messages.d, States.E);
            //machine.Add(States.E, Messages.e, States.A);

            machine.PostMessage(Messages.a);
            Assert.AreEqual(machine.State, States.B);

            machine.PostMessage(Messages.b);
            Assert.AreEqual(machine.State, States.C);

            machine.PostMessage(Messages.c);
            Assert.AreEqual(machine.State, States.D);

            machine.PostMessage(Messages.d);
            Assert.AreEqual(machine.State, States.E);
        }

        [TestMethod]
        public void TestStateEnterActions()
        {
            MessageQueue q = new MessageQueue();
            var machine = new StateMachine<States, Messages, object>(q, States.A, "");
            StringBuilder sb = new StringBuilder();

            machine.Add(States.A, Messages.a, States.B);
            machine.Add(States.B, Messages.b, States.C);
            machine.Add(States.C, Messages.c, States.D);
            machine.Add(States.D, Messages.d, States.E);
            machine.Add(States.E, Messages.e, States.A);

            machine.SetStateEnterEvent(States.A, (fromState, toState, payload) => sb.Append("1"));
            machine.SetStateEnterEvent(States.B, (fromState, toState, payload) => sb.Append("2"));
            machine.SetStateEnterEvent(States.C, (fromState, toState, payload) => sb.Append("3"));
            machine.SetStateEnterEvent(States.D, (fromState, toState, payload) => sb.Append("4"));
            machine.SetStateEnterEvent(States.E, (fromState, toState, payload) => sb.Append("5"));

            machine.PostMessage(Messages.a);
            Assert.AreEqual(machine.State, States.B);

            machine.PostMessage(Messages.b);
            Assert.AreEqual(machine.State, States.C);

            machine.PostMessage(Messages.c);
            Assert.AreEqual(machine.State, States.D);

            machine.PostMessage(Messages.d);
            Assert.AreEqual(machine.State, States.E);

            machine.PostMessage(Messages.e);
            Assert.AreEqual(machine.State, States.A);

            Assert.AreEqual("23451", sb.ToString());
        }

        [TestMethod]
        public void TestStateLeaveActions()
        {
            MessageQueue q = new MessageQueue();
            var machine = new StateMachine<States, Messages, object>(q, States.A, "");
            StringBuilder sb = new StringBuilder();

            machine.Add(States.A, Messages.a, States.B);
            machine.Add(States.B, Messages.b, States.C);
            machine.Add(States.C, Messages.c, States.D);
            machine.Add(States.D, Messages.d, States.E);
            machine.Add(States.E, Messages.e, States.A);

            machine.SetStateLeaveEvent(States.A, (fromState, toState, payload) => sb.Append("A"));
            machine.SetStateLeaveEvent(States.B, (fromState, toState, payload) => sb.Append("B"));
            machine.SetStateLeaveEvent(States.C, (fromState, toState, payload) => sb.Append("C"));
            machine.SetStateLeaveEvent(States.D, (fromState, toState, payload) => sb.Append("D"));
            machine.SetStateLeaveEvent(States.E, (fromState, toState, payload) => sb.Append("E"));

            machine.PostMessage(Messages.a);
            Assert.AreEqual(machine.State, States.B);

            machine.PostMessage(Messages.b);
            Assert.AreEqual(machine.State, States.C);

            machine.PostMessage(Messages.c);
            Assert.AreEqual(machine.State, States.D);

            machine.PostMessage(Messages.d);
            Assert.AreEqual(machine.State, States.E);

            machine.PostMessage(Messages.e);
            Assert.AreEqual(machine.State, States.A);

            Assert.AreEqual("ABCDE", sb.ToString());
        }


        [TestMethod]
        public void TestStateLeaveAndEnterActions()
        {
            MessageQueue q = new MessageQueue();
            var machine = new StateMachine<States, Messages, object>(q, States.A, "");
            StringBuilder sb = new StringBuilder();

            machine.Add(States.A, Messages.a, States.B);
            machine.Add(States.B, Messages.b, States.C);
            machine.Add(States.C, Messages.c, States.D);
            machine.Add(States.D, Messages.d, States.E);
            machine.Add(States.E, Messages.e, States.A);

            machine.SetStateEnterEvent(States.A, (fromState, toState, payload) => sb.Append("1"));
            machine.SetStateEnterEvent(States.B, (fromState, toState, payload) => sb.Append("2"));
            machine.SetStateEnterEvent(States.C, (fromState, toState, payload) => sb.Append("3"));
            machine.SetStateEnterEvent(States.D, (fromState, toState, payload) => sb.Append("4"));
            machine.SetStateEnterEvent(States.E, (fromState, toState, payload) => sb.Append("5"));

            machine.SetStateLeaveEvent(States.A, (fromState, toState, payload) => sb.Append("A"));
            machine.SetStateLeaveEvent(States.B, (fromState, toState, payload) => sb.Append("B"));
            machine.SetStateLeaveEvent(States.C, (fromState, toState, payload) => sb.Append("C"));
            machine.SetStateLeaveEvent(States.D, (fromState, toState, payload) => sb.Append("D"));
            machine.SetStateLeaveEvent(States.E, (fromState, toState, payload) => sb.Append("E"));

            machine.PostMessage(Messages.a);
            Assert.AreEqual(machine.State, States.B);

            machine.PostMessage(Messages.b);
            Assert.AreEqual(machine.State, States.C);

            machine.PostMessage(Messages.c);
            Assert.AreEqual(machine.State, States.D);

            machine.PostMessage(Messages.d);
            Assert.AreEqual(machine.State, States.E);

            machine.PostMessage(Messages.e);
            Assert.AreEqual(machine.State, States.A);

            Assert.AreEqual("A2B3C4D5E1", sb.ToString());
        }

        [TestMethod]
        public void TestStateEnterActionsMultipleRegistrations()
        {
            MessageQueue q = new MessageQueue();
            var machine = new StateMachine<States, Messages, object>(q, States.A, "");
            StringBuilder sb = new StringBuilder();

            machine.Add(States.A, Messages.a, States.B);
            machine.Add(States.B, Messages.b, States.C);
            machine.Add(States.C, Messages.c, States.D);
            machine.Add(States.D, Messages.d, States.E);
            machine.Add(States.E, Messages.e, States.A);

            // 
            machine.Add(States.A, Messages.b, States.B);
            machine.Add(States.A, Messages.c, States.B);
            machine.Add(States.A, Messages.d, States.B);
            machine.Add(States.A, Messages.e, States.B);

            machine.Add(States.B, Messages.a, States.B);
            machine.Add(States.B, Messages.c, States.B);
            machine.Add(States.B, Messages.d, States.B);
            machine.Add(States.B, Messages.e, States.B);
            
            machine.Add(States.C, Messages.a, States.B);
            machine.Add(States.C, Messages.b, States.B);
            machine.Add(States.C, Messages.d, States.B);
            machine.Add(States.C, Messages.e, States.B);

            machine.SetStateEnterEvent(States.A, (fromState, toState, payload) => sb.Append("1"));
            machine.SetStateEnterEvent(States.B, (fromState, toState, payload) => sb.Append("2"));
            machine.SetStateEnterEvent(States.C, (fromState, toState, payload) => sb.Append("3"));
            machine.SetStateEnterEvent(States.D, (fromState, toState, payload) => sb.Append("4"));
            machine.SetStateEnterEvent(States.E, (fromState, toState, payload) => sb.Append("5"));


            machine.PostMessage(Messages.a);
            Assert.AreEqual(machine.State, States.B);

            machine.PostMessage(Messages.b);
            Assert.AreEqual(machine.State, States.C);

            machine.PostMessage(Messages.c);
            Assert.AreEqual(machine.State, States.D);

            machine.PostMessage(Messages.d);
            Assert.AreEqual(machine.State, States.E);

            machine.PostMessage(Messages.e);
            Assert.AreEqual(machine.State, States.A);

            Assert.AreEqual("23451", sb.ToString());
        }

        [TestMethod]
        public void TestStateEnterActionsMultipleRegistrationsMixedUp()
        {
            MessageQueue q = new MessageQueue();
            var machine = new StateMachine<States, Messages, object>(q, States.A, "");
            StringBuilder sb = new StringBuilder();

            machine.SetStateEnterEvent(States.A, (fromState, toState, payload) => sb.Append("1"));

            machine.Add(States.A, Messages.a, States.B);
            machine.Add(States.B, Messages.b, States.C);
            machine.Add(States.C, Messages.c, States.D);
            machine.Add(States.D, Messages.d, States.E);
            machine.Add(States.E, Messages.e, States.A);

            machine.SetStateEnterEvent(States.B, (fromState, toState, payload) => sb.Append("2"));
            machine.SetStateEnterEvent(States.C, (fromState, toState, payload) => sb.Append("3"));
            // 
            machine.Add(States.A, Messages.b, States.B);
            machine.Add(States.A, Messages.c, States.B);
            machine.Add(States.A, Messages.d, States.B);
            machine.Add(States.A, Messages.e, States.B);

            machine.SetStateEnterEvent(States.D, (fromState, toState, payload) => sb.Append("4"));

            machine.Add(States.B, Messages.a, States.B);
            machine.Add(States.B, Messages.c, States.B);
            machine.Add(States.B, Messages.d, States.B);
            machine.Add(States.B, Messages.e, States.B);

            machine.SetStateEnterEvent(States.E, (fromState, toState, payload) => sb.Append("5"));

            machine.Add(States.C, Messages.a, States.B);
            machine.Add(States.C, Messages.b, States.B);
            machine.Add(States.C, Messages.d, States.B);
            machine.Add(States.C, Messages.e, States.B);



            machine.PostMessage(Messages.a);
            Assert.AreEqual(machine.State, States.B);

            machine.PostMessage(Messages.b);
            Assert.AreEqual(machine.State, States.C);

            machine.PostMessage(Messages.c);
            Assert.AreEqual(machine.State, States.D);

            machine.PostMessage(Messages.d);
            Assert.AreEqual(machine.State, States.E);

            machine.PostMessage(Messages.e);
            Assert.AreEqual(machine.State, States.A);

            Assert.AreEqual("23451", sb.ToString());
        }
    }
}
