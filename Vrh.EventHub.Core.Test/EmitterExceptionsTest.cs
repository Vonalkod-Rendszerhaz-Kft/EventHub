using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vrh.EventHub.Core.Exceptions;

namespace Vrh.EventHub.Core.Test
{
    [TestClass]
    public class EmitterExceptionsTest
    {
        [TestMethod]
        public void EmiterExceptions_EmptyEmitterIdNotPosibleException()
        {
            EmptyEmitterIdNotPosibleException ex = new EmptyEmitterIdNotPosibleException();
            Assert.IsInstanceOfType(ex, typeof(FatalEventHubException));
        }

        [TestMethod]
        public void EmitterExceptions_EmitterIdAlreadySettedException()
        {
            EmitterIdAlreadySettedException ex = new EmitterIdAlreadySettedException();
            Assert.IsInstanceOfType(ex, typeof(FatalEventHubException));
        }
    }
}
