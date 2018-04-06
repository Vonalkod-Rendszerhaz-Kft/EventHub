using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Vrh.EventHub.Core;
using Vrh.UnitTest.Base;

namespace Vrh.EventHub.Core.Test
{
    [TestClass]
    public class RequestTest : VrhUnitTestBaseClass
    {
        [TestMethod]
        public void RequestCreate_ContentTypeSafe()
        {
            Assert.IsInstanceOfType(_request.RequestContent, typeof(int));
        }


        [TestMethod]
        public void RequestCreate_IdProperty()
        {
            Assert.AreEqual("M1", _request.Id);
        }

        [TestMethod]
        public void RequestCreate_MessageContentProperty()
        {
            Assert.AreEqual(1, _request.RequestContent);
        }

        [TestMethod]
        public void RequestGetMyResponse_ResponseTypeSafe()
        {
            Assert.IsInstanceOfType(_request.MyResponse, typeof(Response<bool>));
        }

        [TestMethod]
        public void RequestGetMyResponse_ResponseMessageContentTypeSafe()
        {
            Assert.IsInstanceOfType(_request.MyResponse.ResponseContent, typeof(bool));
        }

        [TestMethod]
        public void RequestGetMyResponse_ResponseExceptionInitWithNull()
        {
            Assert.IsNull(_request.MyResponse.Exception);
        }

        [TestMethod]
        public void RequestGetMyResponse_ResponseInitOk()
        {
            Assert.IsTrue(_request.MyResponse.Ok);
        }

        [TestMethod]
        public void RequestGetMyResponse_RequestIdInit()
        {
            Assert.AreEqual(_request.Id, _request.MyResponse.RequestId);
        }

        [TestMethod]
        public void RequestGetMyResponse_RequestMessageContentInit()
        {
            Assert.AreEqual(default(bool), _request.MyResponse.ResponseContent);
        }

        [TestMethod]
        public void RequestSerializable()
        {
            Assert.IsInstanceOfType(Clone, typeof(Request<int, bool>));
        }

        [TestMethod]
        public void RequestSerializable_MessageContentTypeSafe()
        {
            Assert.IsInstanceOfType(Clone.RequestContent, typeof(int));
        }

        [TestMethod]
        public void RequestSerializable_IdProperty()
        {
            Assert.AreEqual(_request.Id, Clone.Id);
        }

        [TestMethod]
        public void RequestSerializable_MessageContentProperty()
        {
            Assert.AreEqual(_request.RequestContent, Clone.RequestContent);
        }

        [TestMethod]
        public void RequestSerializable_GetMyResponse_TypeSafe()
        {
            Assert.IsInstanceOfType(Clone.MyResponse, typeof(Response<bool>));
        }

        [TestMethod]
        public void RequestSerializable_GetMyResponse_MessageContentTypeSafe()
        {
            Assert.IsInstanceOfType(Clone.MyResponse.ResponseContent, typeof(bool));
        }

        [TestMethod]
        public void RequestSerializable_GetMyResponse_MessageContentInit()
        {
            Assert.AreEqual(default(bool), Clone.MyResponse.ResponseContent);
        }

        [TestMethod]
        public void RequestSerializable_GetMyResponse_ExceptionInit()
        {
            Assert.IsNull(_request.MyResponse.Exception);
        }

        [TestMethod]
        public void RequestSerializable_GetMyResponse_StatusInit()
        {
            Assert.IsTrue(_request.MyResponse.Ok);
        }

        [TestMethod]
        public void RequestSerializable_GetMyResponse_RequestIdInit()
        {
            Assert.AreEqual(_request.Id, _request.MyResponse.RequestId);
        }                          

        private Request<int, bool> Clone
        {
            get
            {
                lock (_cloneLocker)
                {
                    if (_clone == null)
                    {
                        string json = JsonConvert.SerializeObject(_request);
                        _clone = JsonConvert.DeserializeObject<Request<int, bool>>(json);
                    }
                }
                return _clone;
            }
        }

        private Request<int, bool> _clone = null;

        private object _cloneLocker = new object();

        private Request<int, bool> _request = new Request<int, bool>() { Id = "M1", RequestContent = 1 };
    }
}
