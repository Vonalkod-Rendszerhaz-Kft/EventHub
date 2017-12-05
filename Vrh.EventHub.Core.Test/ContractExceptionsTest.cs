using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vrh.EventHub.Core.Exceptions;

namespace Vrh.EventHub.Core.Test
{
    [TestClass]
    public class ContractExceptionsTest
    {
        [TestMethod]
        public void ContractException_ContractNotContainsException()
        {
            var ex = new ContractNotContainsException<String>(typeof(Int32));
            Assert.IsInstanceOfType(ex, typeof(FatalEventHubException));
            Assert.IsTrue(ex.Message.Contains(typeof(String).FullName));
            Assert.IsTrue(ex.Message.Contains(typeof(Int32).FullName));
        }
    }
}
