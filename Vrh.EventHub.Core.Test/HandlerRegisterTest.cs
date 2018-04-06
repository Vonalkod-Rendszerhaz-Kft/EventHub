using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vrh.UnitTest.Base;
using Vrh.EventHub.Core;

namespace Vrh.EventHub.Core.Test
{
    [TestClass]
    public class HandlerRegisterTest :VrhUnitTestBaseClass
    {
        public HandlerRegisterTest()
        {
            _hrFunc = HandlerRegister.RegistHandler<int, bool>(TestHandlerFunc);
            _hrAction = HandlerRegister.RegistHandler<int>(TestHandlerAction);
        }

        [TestMethod]
        public void HandlerRegister_Disposable()
        {
            Assert.IsInstanceOfType(_hrFunc, typeof(IDisposable));   
        }

        [TestMethod]
        public void HandlerRegister_DisposOk()
        {
            var hr = HandlerRegister.RegistHandler<int, bool>(TestHandlerFunc);
            Assert.IsNotNull(hr.Handler);
            hr.Dispose();
            Assert.IsNull(hr.Handler);
        }

        [TestMethod]
        public void HandlerRegister_HandlerRegisteredAndWorkWithFunc()
        {
            Assert.IsInstanceOfType(
                _hrFunc.Handler.Method.Invoke(_hrFunc.Handler.Target,
                    new object[] {new Request<int, bool> {Id = "M1", RequestContent = 1}}),
                typeof(Response<bool>));
        }

        [TestMethod]
        public void HandlerRegister_HandlerRegisteredAndWorkWithAction()
        {
            _hrAction.Handler.Method.Invoke(_hrAction.Handler.Target,
                new object[] { 1 });
        }

        private Response<bool> TestHandlerFunc(Request<int, bool> req)
        {
            return req.MyResponse;
        }

        private void TestHandlerAction(int req)
        {
            Assert.AreEqual(1, req);
        }

        private HandlerRegister _hrFunc;    
        private HandlerRegister _hrAction;
    }
}
