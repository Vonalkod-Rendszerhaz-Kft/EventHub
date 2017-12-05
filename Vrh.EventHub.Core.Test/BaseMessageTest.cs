using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vrh.EventHub.Core.Interfaces;
using Vrh.EventHub.Core.Exceptions;

namespace Vrh.EventHub.Core.Test
{
    [TestClass]
    public class BaseMessageTest
    {
        [TestMethod]       
        public void BaseMessage_CallbackMessage()
        {
            var m = new TestMessage1();
            Assert.AreEqual(m.CallbackMessage.FullName, typeof(TestCallBack).FullName);
        }

        [TestMethod]
        [ExpectedException(typeof(BadMessageTypeException))]
        public void BaseMessage_BadCallbackMessage()
        {
            var m = new BadMessage();
            var cm = m.CallbackMessage;
        }

        [TestMethod]
        public void BaseMessage_Description()
        {
            var m = new TestMessage1();
            Assert.AreEqual(TestMessage1.MESSAGE_DESCRIPTION, m.Description);
        }

        [TestMethod]
        public void BaseMessage_IsCallback()
        {
            var m = new TestMessage1();
            Assert.IsFalse(m.IsCallback);
            var cm = new TestCallBack();
            Assert.IsTrue(cm.IsCallback);
        }

        [TestMethod]
        public void BaseMessage_Key()
        {
            var m = new TestMessage1();
            Assert.AreEqual(typeof(TestMessage1).Name, m.Id);
        }
    }

    [MessageMeta("Default response message")]
    public class TestCallBack : BaseMessage
    {
        public bool IsOK { get; set; }

        public string Message { get; set; }
    }

    [MessageMeta(TestMessage1.MESSAGE_DESCRIPTION, typeof(TestCallBack))]
    public class TestMessage1 : BaseMessage
    {
        public string ProductItem { get; set; }

        public int Qty { get; set; }

        public const string MESSAGE_DESCRIPTION = "Increase Count of production";
    }    

    [MessageMeta("Bad callback (not derived from BaseMEssage)", typeof(String))]
    public class BadMessage : BaseMessage
    {
    }

}
