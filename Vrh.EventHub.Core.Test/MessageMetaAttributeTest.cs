using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vrh.EventHub.Core.Interfaces;
using Vrh.EventHub.Core.Exceptions;

namespace Vrh.EventHub.Core.Test
{
    [TestClass]
    public class MessageMetaAttributeTest
    {
        [TestMethod]
        public void MessageMetaAttribute_IsAttribute()
        {
            Assert.IsTrue(new MessageMetaAttribute("d") is Attribute);
        }

        [TestMethod]
        public void MessageMetaAttribute_Description()
        {
            var mma = new MessageMetaAttribute("d");
            Assert.AreEqual("d", mma.Description);
        }

        [TestMethod]
        public void MessageMetaAttribute_CallbackMessage()
        {
            var mma = new MessageMetaAttribute("d", typeof(TestCallBack));
            Assert.AreEqual(typeof(TestCallBack), mma.CallbackMessage);
        }

        [TestMethod]
        [ExpectedException(typeof(BadMessageTypeException))]
        public void MessageMetaAttribute_CallbackMessageMustDerivedBaseMessage()
        {
            var mma = new MessageMetaAttribute("d", typeof(String));
        }

        [TestMethod]
        public void MessageMetaAttribute_IsNotCallback()
        {
            var mma = new MessageMetaAttribute("d1", typeof(TestCallBack));
            var mma2 = new MessageMetaAttribute("d2", typeof(TestCallBack), false);
            var mma3 = new MessageMetaAttribute("d3", isCallback: false);
            Assert.IsFalse(mma.IsCallback);
            Assert.IsFalse(mma2.IsCallback);
            Assert.IsFalse(mma3.IsCallback);
        }

        [TestMethod]
        public void MessageMetaAttribute_IsCallback()
        {
            var mma = new MessageMetaAttribute("d1");
            var mma2 = new MessageMetaAttribute("d2", null, true);
            Assert.IsTrue(mma.IsCallback);
            Assert.IsTrue(mma2.IsCallback);
        }
    }
}
