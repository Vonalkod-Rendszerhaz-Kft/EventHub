using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vrh.EventHub.Core.Interfaces;
using Vrh.EventHub.Core.Exceptions;

namespace Vrh.EventHub.Core.Test
{
    [TestClass]
    public class BaseEmitterTest
    {
        [TestMethod]
        public void BaseEmitter_EmitterType()
        {
            IEmitter e = new BaseEmitter();
            Assert.AreEqual(typeof(BaseEmitter).FullName, e.EmitterType);
        }

        [TestMethod]
        public void BaseEmitter_Id()
        {
            IEmitter e = new BaseEmitter();
            e.Id = typeof(BaseEmitter).Name;
            Assert.AreEqual(typeof(BaseEmitter).Name, e.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(EmptyEmitterIdNotPosibleException))]
        public void BaseEmitter_Id_MustNotNull()
        {
            IEmitter e = new BaseEmitter();
            e.Id = null;
        }

        [TestMethod]
        [ExpectedException(typeof(EmptyEmitterIdNotPosibleException))]
        public void BaseEmitter_Id_MustNotEmpty()
        {
            IEmitter e = new BaseEmitter();
            e.Id = String.Empty;
        }

        [TestMethod]
        [ExpectedException(typeof(EmitterIdAlreadySettedException))]
        public void BaseEmitter_Id_SingleSetteble()
        {
            IEmitter e = new BaseEmitter();
            e.Id = typeof(BaseEmitter).Name;
            Assert.AreEqual(typeof(BaseEmitter).Name, e.Id);
            e.Id = "new id";
        }

        [TestMethod]
        public void BaseEmitter_Description()
        {
            IEmitter e = new BaseEmitter();
            Assert.AreEqual(null, e.Description);
        }

        [TestMethod]
        public void BaseEmitter_SendMessage()
        {            
            var d = typeof(BaseEmitter).GetMethod("SendMessage");
            Assert.IsTrue(d.IsVirtual);
            IEmitter e = new BaseEmitter();
            bool notImplemented = false;
            try
            {
                e.SendMessage(new TestMessage1());
            }
            catch (NotImplementedException)
            {
                notImplemented = true;
            }
            Assert.IsTrue(notImplemented);
        }

        [TestMethod]
        public void BaseEmitter_DropMessage()
        {
            var d = typeof(BaseEmitter).GetMethod("DropMessage");
            Assert.IsTrue(d.IsVirtual);
            IEmitter e = new BaseEmitter();
            bool notImplemented = false;
            try
            {
                e.DropMessage(new TestMessage1());
            }
            catch (NotImplementedException)
            {
                notImplemented = true;
            }
            Assert.IsTrue(notImplemented);
        }

    }
}
