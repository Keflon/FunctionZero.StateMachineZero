
using System;
using System.Reflection.PortableExecutable;
using FunctionZero.StateMachineZero;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StateMachineZeroTests
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


    [TestClass]
    public class UnitTest1
    {
	    [TestMethod]
	    public void TestBasic()
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
	}
}
