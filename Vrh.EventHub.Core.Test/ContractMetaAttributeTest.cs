using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vrh.EventHub.Core.Interfaces;

namespace Vrh.EventHub.Core.Test
{
    [TestClass]
    public class ContractMetaAttributeTest
    {
        [TestMethod]
        public void ContractMetaAttribute_IsAttribute()
        {
            Assert.IsTrue(new ContractMetaAttribute("d") is Attribute);
        }

        [TestMethod]
        public void ContractMetaAttribute_Description()
        {
            var cma = new ContractMetaAttribute("d");
            Assert.AreEqual("d", cma.Description);
        }
    }
}
