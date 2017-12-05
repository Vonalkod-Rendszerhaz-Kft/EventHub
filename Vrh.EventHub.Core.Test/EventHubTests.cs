using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vrh.EventHub.Core;
using Vrh.EventHub.Core.Interfaces;

namespace Vrh.EventHub.Core.Test
{
    [TestClass]
    public class EventHubTests
    {
        [TestMethod]
        public void EventHub_GetEmitter()
        {
            IEmitter e = EventHub.GetEmitter<BaseChannel, TestContract>("e1");
            Assert.IsNotNull(e);
            Assert.AreEqual("e1", e.Id);
        }

        [TestMethod]
        public void EventHub_GetCollector()
        {
            ICollector e = EventHub.GetCollector<BaseChannel, TestContract>("c1");
            Assert.IsNotNull(e);
            Assert.AreEqual("c1", e.Id);
        }
    }
}
