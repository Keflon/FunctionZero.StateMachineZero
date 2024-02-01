using System.Text;
using FunctionZero.StateMachineZero;

namespace StateMachineZeroTests
{
    [TestClass]
    public class TwoMachinesOneQueue
    {
        enum StatesA
        {
            NoneA,
            AS0,
            AS1,
            AS2,
            AS3,
            AS4,
            AS5
        }

        enum MessagesA
        {
            AM0,
            AM1,
            AM2,
            AM3,
            AM4,
            AM5,
            AM6
        }

        enum StatesB
        {
            NoneB,
            BS0,
            BS1,
            BS2,
            BS3,
            BS4,
            BS5
        }

        enum MessagesB
        {
            BM0,
            BM1,
            BM2,
            BM3,
            BM4,
            BM5,
            BM6
        }

        private StateMachine<StatesA, MessagesA, StringBuilder> _machineA;
        private StateMachine<StatesB, MessagesB, StringBuilder> _machineB;

        [TestMethod]
        public void TestTwoMachines()
        {
            MessageQueue q = new MessageQueue();
            _machineA = GetDefaultMachineA(q);
            _machineB = GetDefaultMachineB(q);

            _machineA.StateChanged += MachineA_StateChanged;
            _machineB.StateChanged += MachineBOnStateChanged;

            _machineA.BadTransition += MachineAOnBadTransition;
            _machineB.BadTransition += MachineBOnBadTransition;

            StringBuilder sb = new StringBuilder();
            _machineA.PostMessage(MessagesA.AM0, sb);

            Assert.AreEqual(sb.ToString(), "AS0 BS0 BS1 BS2 AS1 AS2 AS3 BS3 AS4 AS5 Fault:AS5;AM4 BS4 ");

        }

        private void MachineAOnBadTransition(object sender, BadTransitionEventArgs<StatesA, MessagesA, StringBuilder> e)
        {
            e.PayLoad.Append("Fault:" + e.CurrentState + ";" + e.Message + " ");
        }

        private void MachineBOnBadTransition(object sender, BadTransitionEventArgs<StatesB, MessagesB, StringBuilder> e)
        {
            e.PayLoad.Append("Fault:" + e.CurrentState + ";" + e.Message + " ");
        }

        private void MachineA_StateChanged(object sender, StateChangeEventArgs<StatesA, MessagesA, StringBuilder> e)
        {
            e.PayLoad.Append(e.NewState + " ");

            switch (e.NewState)
            {
                case StatesA.AS0:
                    _machineB.PostMessage(MessagesB.BM0, e.PayLoad);
                    _machineB.PostMessage(MessagesB.BM1, e.PayLoad);
                    break;
                case StatesA.AS1:
                    break;
                case StatesA.AS2:
                    _machineA.PostMessage(MessagesA.AM3, e.PayLoad);
                    _machineB.PostMessage(MessagesB.BM3, e.PayLoad);
                    break;
                case StatesA.AS3:
                    _machineA.PostMessage(MessagesA.AM4, e.PayLoad);
                    break;
                case StatesA.AS4:
                    _machineA.PostMessage(MessagesA.AM4, e.PayLoad);
                    break;
                case StatesA.AS5:
                    _machineB.PostMessage(MessagesB.BM4, e.PayLoad);
                    break;
            }
        }

        private void MachineBOnStateChanged(object sender, StateChangeEventArgs<StatesB, MessagesB, StringBuilder> e)
        {
            e.PayLoad.Append(e.NewState + " ");

            switch (e.NewState)
            {
                case StatesB.BS0:
                    break;
                case StatesB.BS1:
                    _machineB.PostMessage(MessagesB.BM2, e.PayLoad);
                    break;
                case StatesB.BS2:
                    _machineA.PostMessage(MessagesA.AM1, e.PayLoad);
                    _machineA.PostMessage(MessagesA.AM2, e.PayLoad);
                    break;
                case StatesB.BS3:
                    _machineA.PostMessage(MessagesA.AM5, e.PayLoad);
                    break;
                case StatesB.BS4:
                    break;
            }
        }

        private static StateMachine<StatesA, MessagesA, StringBuilder> GetDefaultMachineA(MessageQueue q)
        {
            var machineA = new StateMachine<StatesA, MessagesA, StringBuilder>(q, StatesA.NoneA, "I am machine A");

            machineA.Add(StatesA.NoneA, MessagesA.AM0, StatesA.AS0);
            machineA.Add(StatesA.AS0, MessagesA.AM1, StatesA.AS1);
            machineA.Add(StatesA.AS1, MessagesA.AM2, StatesA.AS2);
            machineA.Add(StatesA.AS2, MessagesA.AM3, StatesA.AS3);
            machineA.Add(StatesA.AS3, MessagesA.AM4, StatesA.AS4);
            machineA.Add(StatesA.AS4, MessagesA.AM5, StatesA.AS5);
            machineA.Add(StatesA.AS5, MessagesA.AM6, StatesA.AS0);

            return machineA;
        }

        private static StateMachine<StatesB, MessagesB, StringBuilder> GetDefaultMachineB(MessageQueue q)
        {
            var machineB = new StateMachine<StatesB, MessagesB, StringBuilder>(q, StatesB.NoneB, "I am machine B");

            machineB.Add(StatesB.NoneB, MessagesB.BM0, StatesB.BS0);
            machineB.Add(StatesB.BS0, MessagesB.BM1, StatesB.BS1);
            machineB.Add(StatesB.BS1, MessagesB.BM2, StatesB.BS2);
            machineB.Add(StatesB.BS2, MessagesB.BM3, StatesB.BS3);
            machineB.Add(StatesB.BS3, MessagesB.BM4, StatesB.BS4);
            machineB.Add(StatesB.BS4, MessagesB.BM5, StatesB.BS5);
            machineB.Add(StatesB.BS5, MessagesB.BM6, StatesB.BS0);

            return machineB;
        }
    }
}
