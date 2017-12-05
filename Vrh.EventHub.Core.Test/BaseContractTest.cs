using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vrh.EventHub.Core.Exceptions;
using Vrh.EventHub.Core.Interfaces;
using VRH.Common;

namespace Vrh.EventHub.Core.Test
{
    [TestClass]
    public class BaseContractTest
    {
        [TestMethod]
        public void BaseContract_Key()
        {
            var tc = new TestContract();
            Assert.AreEqual(typeof(TestContract).FullName, tc.Id);
        }

        [TestMethod]
        public void BaseContract_Version()
        {
            var tc = new TestContract();
            Assert.AreEqual(typeof(TestContract).Assembly.Version(), tc.Version);
        }

        [TestMethod]
        public void BaseContract_Description()
        {
            var tc = new TestContract();
            Assert.AreEqual(TestContract.CONTRACT_DESCRIPTION, tc.Description);
        }

        [TestMethod]
        public void BaseContract_ContractAssembly()
        {
            var tc = new TestContract();
            Assert.AreEqual(typeof(TestContract).Assembly.Location, tc.ContractAssembly);
        }

        [TestMethod]
        public void BaseContract_HandledMessages()
        {
            var tc = new TestContract();
            Assert.AreEqual(1, tc.HandledMessages.Count);
            Assert.AreEqual(typeof(TestMessage1).Name, tc.HandledMessages[0].MessageNema);
            Assert.AreEqual(typeof(TestCallBack).Name, tc.HandledMessages[0].CallbackName);
        }

        [TestMethod]
        public void BaseContract_HandledMessageNames()
        {
            var tc = new TestContract();
            Assert.AreEqual(1, tc.HandledMessageNames.Count);
            Assert.AreEqual(typeof(TestMessage1).Name, tc.HandledMessageNames[0]);
        }

        [TestMethod]
        public void BaseContract_HandledCallbackNames()
        {
            var tc = new TestContract();
            Assert.AreEqual(1, tc.HandledCallbackNames.Count);
            Assert.AreEqual(typeof(TestCallBack).Name, tc.HandledCallbackNames[0]);
        }

        [TestMethod]
        public void BaseContract_GetEmptyMessageInstance()
        {
            var tc = new TestContract();
            var msg = tc.GetEmptyMessageInstance<TestMessage1>();
            Assert.IsInstanceOfType(msg, typeof(TestMessage1));
        }

        [TestMethod]
        [ExpectedException(typeof(ContractNotContainsException<TestCallBack>))]
        public void BaseContract_GetEmptyMessageInstanceContrantNotContrains()
        {
            var tc = new TestContract();
            var msg = tc.GetEmptyMessageInstance<TestCallBack>();
            Assert.IsInstanceOfType(msg, typeof(TestCallBack));
        }

        [TestMethod]
        public void BaseContract_GetEmptyCallbackInstance()
        {
            var tc = new TestContract();
            var msg = tc.GetEmptyCallbackInstance<TestCallBack>();
            Assert.IsInstanceOfType(msg, typeof(TestCallBack));
        }

        [TestMethod]
        [ExpectedException(typeof(ContractNotContainsException<TestMessage1>))]
        public void BaseContract_GetEmptyCallbackInstanceContrantNotContrains()
        {
            var tc = new TestContract();
            var msg = tc.GetEmptyCallbackInstance<TestMessage1>();
            Assert.IsInstanceOfType(msg, typeof(TestMessage1));
        }


        [TestMethod]
        public void BaseContract_MessageWithCallbackToString()
        {
            var mwc = new MessageWithCallback()
            {
                CallbackName = "C",
                MessageNema = "M",
            };
            Assert.AreEqual($"{mwc.MessageNema} --> {mwc.CallbackName}", mwc.ToString());
        }

    }

    [Interfaces.ContractMeta(TestContract.CONTRACT_DESCRIPTION)]
    public class TestContract : BaseContract
    {
        public TestContract()
        {
        }

        public TestCallBack PartOfContractAsCallback { get; set; }

        public TestMessage1 PartOfContractAsMessage { get; set; }

        public String ThisIsNotPartOfContract_bc_not_IMessage { get; set; }

        public String ThisIsNotPartOfContract_bc_not_BaseMessage { get; set; }

        internal const string CONTRACT_DESCRIPTION = "This is a test contract for Event hub";
    }

    public class ThisIsNotPartOfContract_bc_not_BaseMessage : IMessage
    {
        public Type CallbackMessage
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string Description
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsCallback
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string Id
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
