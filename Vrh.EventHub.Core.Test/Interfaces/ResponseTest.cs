using System;
using System.Runtime.Remoting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vrh.EventHub.Core;
using Vrh.UnitTest.Base;
using Newtonsoft.Json;

namespace Vrh.EventHub.Core.Test
{
    [TestClass]
    public class ResponseTest : VrhUnitTestBaseClass
    {
        [TestMethod]
        public void ResponseFactory_IsTypeSafe()
        {
            Assert.IsInstanceOfType(_response, typeof(Response<int>));            
        }

        [TestMethod]
        public void ResponseFactory_ContentIsTypeSafe()
        {
            Assert.IsInstanceOfType(_response.ResponseContent, typeof(int));
        }

        [TestMethod]
        public void ResponseFactory_RequestIdPropertyInit()
        {
            Assert.AreEqual("R1", _response.RequestId);
        }

        [TestMethod]
        public void ResponseFactory_MessageStatusInit()
        {
            Assert.IsTrue(_response.Ok);
        }

        [TestMethod]
        public void ResponseFactory_ExceptionPropertyInit()
        {
            Assert.IsNull(_response.Exception);
        }

        [TestMethod]
        public void ResponseSerializable_CloneNotSame()
        {
            Assert.AreNotSame(_response, Clone);
        }

        [TestMethod]
        public void ResponseSerializable_TypeSafe()
        {
            Assert.IsInstanceOfType(Clone, typeof(Response<int>));
        }

        [TestMethod]
        public void ResponseSerializable_ContentTypeSafe()
        {
            Assert.IsInstanceOfType(Clone.ResponseContent, typeof(int));
        }

        [TestMethod]
        public void ResponseSerializable_RequestIdProperty()
        {
            Assert.AreEqual(_response.RequestId, Clone.RequestId);
        }

        [TestMethod]
        public void ResponseSerializable_Status()
        {
            Assert.AreEqual(_response.Ok, Clone.Ok);
        }

        [TestMethod]
        public void ResponseSerializable_Exception()
        {
            Assert.AreEqual(_response.Exception, Clone.Exception);
        }

        private Response<int> Clone
        {
            get
            {
                lock (_cloneLocker)
                {
                    if (_clone == null)
                    {
                        string json = JsonConvert.SerializeObject(_response);
                        _clone = JsonConvert.DeserializeObject<Response<int>>(json);
                    }
                }
                return _clone;
            }
        }

        private Response<int> _clone = null;

        private object _cloneLocker = new object();

        private Response<int> _response = Response<int>.ResponseFactory("R1");        
    }
}