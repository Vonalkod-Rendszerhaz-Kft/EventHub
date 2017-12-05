using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vrh.EventHub.Core.Interfaces;
using Vrh.EventHub.Core.Exceptions;
using System.Linq;

namespace Vrh.EventHub.Core.Test
{
    [TestClass]
    public class BaseCollectorTest
    {
        [TestMethod]
        public void BaseCollector_CollectorType()
        {
            ICollector c = new BaseCollector();
            Assert.AreEqual(typeof(BaseCollector).FullName, c.CollectorType);
        }

        [TestMethod]
        public void BaseCollector_Id()
        {
            ICollector c = new BaseCollector();
            c.Id = typeof(BaseCollector).Name;
            Assert.AreEqual(typeof(BaseCollector).Name, c.Id);            
        }

        [TestMethod]
        [ExpectedException(typeof(EmptyCollectorIdNotPosibleException))]
        public void BaseCollector_Id_MustNotNull()
        {
            ICollector c = new BaseCollector();
            c.Id = null;
        }

        [TestMethod]
        [ExpectedException(typeof(EmptyCollectorIdNotPosibleException))]
        public void BaseCollector_Id_MustNotEmpty()
        {
            ICollector c = new BaseCollector();
            c.Id = String.Empty;
        }

        [TestMethod]
        [ExpectedException(typeof(CollectorIdAlreadySettedException))]
        public void BaseCollector_Id_SingleSetteble()
        {
            ICollector c = new BaseCollector();
            c.Id = typeof(BaseCollector).Name;
            Assert.AreEqual(typeof(BaseCollector).Name, c.Id);
            c.Id = "new id";
        }

        [TestMethod]
        public void BaseCollector_Description()
        {
            ICollector c = new BaseCollector();            
            Assert.AreEqual(null, c.Description);
        }

        [TestMethod]
        public void BaseCollector_RegisterMessageHandler()
        {
            ICollector c = new BaseCollector();
            c.RegisterMessageHandler<TestMessage1>(TestMessageHandler1);
            Assert.AreEqual(1, ((BaseCollector)c).Handlers.Count);
            Assert.AreEqual(TestMessageHandler1, ((BaseCollector)c).Handlers.FirstOrDefault(x => x.MessageType == typeof(TestMessage1)).Handler);
            c.RegisterMessageHandler<TestMessage1>(TestMessageHandler2);
            Assert.AreEqual(1, ((BaseCollector)c).Handlers.Count);
            Assert.AreEqual(TestMessageHandler2, ((BaseCollector)c).Handlers.FirstOrDefault(x => x.MessageType == typeof(TestMessage1)).Handler);
            c.RegisterMessageHandler<TestCallBack>(TestMessageHandler1);
            Assert.AreEqual(2, ((BaseCollector)c).Handlers.Count);
            Assert.AreEqual(TestMessageHandler1, ((BaseCollector)c).Handlers.FirstOrDefault(x => x.MessageType == typeof(TestCallBack)).Handler);
        }


        private IMessage TestMessageHandler1(IMessage message)
        {
            return message;
        }

        private IMessage TestMessageHandler2(IMessage message)
        {
            return message;
        }

    }
}
