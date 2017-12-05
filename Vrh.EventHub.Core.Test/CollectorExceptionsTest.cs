using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vrh.EventHub.Core.Exceptions;

namespace Vrh.EventHub.Core.Test
{
    [TestClass]
    public class CollectorExceptionsTest
    {
        [TestMethod]
        public void CollectorExceptions_EmptyCollectorIdNotPosibleException()
        {
            var ex = new EmptyCollectorIdNotPosibleException();
            Assert.IsInstanceOfType(ex, typeof(FatalEventHubException));
        }

        [TestMethod]
        public void CollectorExceptions_CollectorIdAlreadySettedException()
        {
            var ex = new CollectorIdAlreadySettedException();
            Assert.IsInstanceOfType(ex, typeof(FatalEventHubException));
        }

    }
}
