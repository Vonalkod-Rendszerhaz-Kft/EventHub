using System;
using System.Runtime.Remoting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vrh.EventHub.Core;
using Vrh.UnitTest.Base;
using System.Threading;
using System.Configuration;

namespace Vrh.EventHub.Core.Test
{
    [TestClass]
    public class CoreExceptionTest : VrhUnitTestBaseClass
    {
        [TestMethod]
        public void EventHubExceptions_Create_EventHubException()
        {
            var ex = new EventHubException("abc");
            Assert.IsInstanceOfType(ex, typeof(Exception));
            Assert.AreEqual($"{EventHubException.EVENTHUB_ERROR_PREFIX}abc", ex.Message);
            var ex2 = new EventHubException("2", ex);
            Assert.AreEqual($"{EventHubException.EVENTHUB_ERROR_PREFIX}2", ex2.Message);
            Assert.AreEqual(ex, ex2.InnerException);
            Assert.AreEqual($"{EventHubException.EVENTHUB_ERROR_PREFIX}abc", ex2.InnerException.Message);
        }

        [TestMethod]
        public void EventHubExceptions_Create_FatalEventHubException()
        {
            FatalEventHubException ex = new FatalEventHubException("abc");
            Assert.IsInstanceOfType(ex, typeof(EventHubException));
            Assert.IsInstanceOfType(ex, typeof(Exception));
            Assert.AreEqual($"{EventHubException.EVENTHUB_ERROR_PREFIX}abc", ex.Message);
            var ex2 = new FatalEventHubException("2", ex);
            Assert.AreEqual($"{EventHubException.EVENTHUB_ERROR_PREFIX}2", ex2.Message);
            Assert.AreEqual(ex, ex2.InnerException);
            Assert.AreEqual($"{EventHubException.EVENTHUB_ERROR_PREFIX}abc", ex2.InnerException.Message);
        }

        [TestMethod]
        public void EventHubExceptions_ThrowOrNo()
        {
            ConfigurationManager.AppSettings[$"{EventHubCore.MODUL_PREFIX}:{EventHubCoreConfig.Me.ThrowEventHubCoreExceptionsElement.NAME}"] 
                = "true";
            bool throwed = false;
            try
            {
                EventHubException.ThrowOrNo(new EventHubException("abc"), this.GetType());
            }
            catch(EventHubException)
            {
                throwed = true;
            }
            Assert.AreEqual(true, throwed);
            ConfigurationManager.AppSettings[$"{EventHubCore.MODUL_PREFIX}:{EventHubCoreConfig.Me.ThrowEventHubCoreExceptionsElement.NAME}"]
                = "false";
            throwed = false;
            try
            {
                EventHubException.ThrowOrNo(new EventHubException("abc"), this.GetType());
            }
            catch (EventHubException)
            {
                throwed = true;
            }
            Assert.AreEqual(false, throwed);
            bool fatalThrow = false;
            try
            {
                EventHubException.ThrowOrNo(new FatalEventHubException("abc"), this.GetType());
            }
            catch (EventHubException)
            {
                fatalThrow = true;
            }
            Assert.IsTrue(fatalThrow);
            Thread.Sleep(1000);
        }
    }
}
