using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vrh.EventHub.Protocols.RedisPubSub;

namespace Vrh.EventHub.Core.Test
{
    [TestClass]
    public class HandlerRegisterTests : Vrh.UnitTest.Base.VrhUnitTestBaseClass
    {
        [TestMethod]
        public void RegisterHandlerWork()
        {
            EventHubCore.RegisterHandler<RedisPubSubChannel, int, bool>(TEST_CHANNEL, TestHandlerFunc);
            Assert.IsTrue(EventHubCore.Call<RedisPubSubChannel, int, bool>(TEST_CHANNEL, 0));
            Assert.IsFalse(EventHubCore.Call<RedisPubSubChannel, int, bool>(TEST_CHANNEL, 1));
        }

        [TestMethod]
        public void DropHandlerWork()
        {
            bool isOK = false;
            EventHubCore.RegisterHandler<RedisPubSubChannel, int, bool>(TEST_CHANNEL, TestHandlerFunc);
            Assert.IsTrue(EventHubCore.Call<RedisPubSubChannel, int, bool>(TEST_CHANNEL, 0));
            EventHubCore.DropHandler<RedisPubSubChannel, int, bool>(TEST_CHANNEL, TestHandlerFunc);
            try
            {
                EventHubCore.Call<RedisPubSubChannel, int, bool>(TEST_CHANNEL, 0);
            }
            catch
            {
                isOK = true;
            }
            Assert.IsTrue(isOK);
        }

        [TestMethod]
        public void OverRegisterWork()
        {            
            EventHubCore.RegisterHandler<RedisPubSubChannel, int, bool>(TEST_CHANNEL, TestHandlerFunc);
            Assert.IsTrue(EventHubCore.Call<RedisPubSubChannel, int, bool>(TEST_CHANNEL, 0));
            EventHubCore.RegisterHandler<RedisPubSubChannel, int, bool>(TEST_CHANNEL, TestHandlerFunc2);
            Assert.IsFalse(EventHubCore.Call<RedisPubSubChannel, int, bool>(TEST_CHANNEL, 0));
        }


        private Response<bool> TestHandlerFunc(Request<int, bool> request)
        {
            var response = request.MyResponse;
            response.ResponseContent = request.RequestContent == 0 ? true : false;
            return response;
        }

        private Response<bool> TestHandlerFunc2(Request<int, bool> request)
        {
            var response = request.MyResponse;
            response.ResponseContent = request.RequestContent != 0 ? true : false;
            return response;
        }

        private const string TEST_CHANNEL = "testchannel";
    }
}
