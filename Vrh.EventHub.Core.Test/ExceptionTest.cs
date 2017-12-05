using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vrh.EventHub.Core.Exceptions;
using Vrh.EventHub.Core;

namespace Vrh.EventHub.Core.Test
{
    [TestClass]
    public class CoreExceptionTest
    {
        [TestMethod]
        public void EventHubExceptions_Create_EventHubException()
        {
            EventHubException ex = new EventHubException("abc");
            Assert.IsInstanceOfType(ex, typeof(Exception));
            Assert.AreEqual("abc", ex.Message);
        }

        [TestMethod]
        public void EventHubExceptions_Create_FatalEventHubException()
        {
            FatalEventHubException ex = new FatalEventHubException("abc");
            Assert.IsInstanceOfType(ex, typeof(EventHubException));
            Assert.IsInstanceOfType(ex, typeof(Exception));
            Assert.AreEqual($"Fatal error: {"abc"}", ex.Message);
        }

        [TestMethod]
        public void EventHubExceptions_ThrowOrNo()
        {
            bool throwNeed = EventHub.EventHubCoreConfiguration.ThrowEventHubCoreExceptions;
            bool throwed = false;
            try
            {
                EventHubException.ThrowOrNo(new EventHubException("abc"), this.GetType());
            }
            catch(EventHubException ex)
            {
                throwed = true;
            }
            Assert.AreEqual(throwNeed, throwed);
            bool fatalThrow = false;
            try
            {
                EventHubException.ThrowOrNo(new FatalEventHubException("abc"), this.GetType());
            }
            catch (EventHubException ex)
            {
                fatalThrow = true;
            }
            Assert.IsTrue(fatalThrow);
        }
    }
}
