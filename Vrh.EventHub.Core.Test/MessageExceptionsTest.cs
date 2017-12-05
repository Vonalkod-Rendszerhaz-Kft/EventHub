using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vrh.EventHub.Core.Exceptions;

namespace Vrh.EventHub.Core.Test
{
    [TestClass]
    public class MessageExceptionsTest
    {
        [TestMethod]
        public void ContractException_ContractNotContainsException()
        {
            var ex = new BadMessageTypeException();
            Assert.IsInstanceOfType(ex, typeof(FatalEventHubException));
        }
    }
}
