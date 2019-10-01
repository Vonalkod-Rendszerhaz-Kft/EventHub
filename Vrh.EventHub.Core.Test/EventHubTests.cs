using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using Vrh.EventHub.Protocols.RedisPubSub;

namespace Vrh.EventHub.Core.Test
{
    [TestClass]
    public class EventHubTests : Vrh.UnitTest.Base.VrhUnitTestBaseClass
    {
        [TestMethod]
        public void CallRegisterHandlerWork()
        {
            EventHubCore.RegisterHandler<RedisPubSubChannel, int, bool>(TEST_CHANNEL, CallTestHandlerFunc);
            Assert.IsTrue(EventHubCore.Call<RedisPubSubChannel, int, bool>(TEST_CHANNEL, 0));
            Assert.IsFalse(EventHubCore.Call<RedisPubSubChannel, int, bool>(TEST_CHANNEL, 1));
        }

        [TestMethod]
        public void SendWorkWithBuiltInRequestWrapper()
        {
            EventHubCore.RegisterHandler<RedisPubSubChannel, int, bool>(TEST_CHANNEL, SendTestHandler, true);
            EventHubCore.Send<RedisPubSubChannel, Request<int, bool>>(TEST_CHANNEL, new Request<int, bool>() { Id = "1", RequestContent = 0 });
            Assert.IsTrue(_semaphore.Wait(5000));
            EventHubCore.Send<RedisPubSubChannel, Request<int, bool>>(TEST_CHANNEL, new Request<int, bool>() { Id = "1", RequestContent = 0 });
            Assert.IsTrue(_semaphore.Wait(5000));
        }

        [TestMethod]
        public void SendWorkWithBuiltInResponseWrapper()
        {
            EventHubCore.RegisterHandler<RedisPubSubChannel, Response<bool>>(TEST_CHANNEL, SendTestHandler3, false);
            EventHubCore.Send<RedisPubSubChannel, Response<bool>>(TEST_CHANNEL, new Response<bool>() { RequestId = "1", ResponseContent = true });
            Assert.IsTrue(_semaphore.Wait(5000));
            EventHubCore.SendAsync<RedisPubSubChannel, Response<bool>>(TEST_CHANNEL, new Response<bool>() { RequestId = "1", ResponseContent = true });
            Assert.IsTrue(_semaphore.Wait(5000));
        }

        [TestMethod]
        public void SendWorkWithCustomType()
        {
            EventHubCore.RegisterHandler<RedisPubSubChannel, TestType>(TEST_CHANNEL, SendTestHandler4, false);
            EventHubCore.Send<RedisPubSubChannel, TestType>(TEST_CHANNEL, new TestType() { Id = "1" });
            Assert.IsTrue(_semaphore.Wait(5000));
            EventHubCore.SendAsync<RedisPubSubChannel, TestType>(TEST_CHANNEL, new TestType() { Id = "1" });
            Assert.IsTrue(_semaphore.Wait(5000));
        }

        [TestMethod]
        public void MultipleRegisterWork()
        {            
            EventHubCore.RegisterHandler<RedisPubSubChannel, int, bool>(OTHER_TEST_CHANNEL, SendTestHandler, false);
            EventHubCore.RegisterHandler<RedisPubSubChannel, int, bool>(OTHER_TEST_CHANNEL, SendTestHandler2, false);
            EventHubCore.Send<RedisPubSubChannel, Request<int, bool>>(OTHER_TEST_CHANNEL, new Request<int, bool>() { Id = "1", RequestContent = 0 });
            Assert.IsTrue(_semaphore.Wait(5000));
            Assert.IsTrue(_semaphore2.Wait(5000));
        }        

        [TestMethod]
        public void DropHandlerWork()
        {
            bool isOK = false;
            EventHubCore.RegisterHandler<RedisPubSubChannel, int, bool>(TEST_CHANNEL, CallTestHandlerFunc);
            Assert.IsTrue(EventHubCore.Call<RedisPubSubChannel, int, bool>(TEST_CHANNEL, 0));
            EventHubCore.DropHandler<RedisPubSubChannel, int, bool>(TEST_CHANNEL, CallTestHandlerFunc);
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
            EventHubCore.RegisterHandler<RedisPubSubChannel, int, bool>(OTHER_TEST_CHANNEL, CallTestHandlerFunc);
            Assert.IsTrue(EventHubCore.Call<RedisPubSubChannel, int, bool>(OTHER_TEST_CHANNEL, 0));
            EventHubCore.RegisterHandler<RedisPubSubChannel, int, bool>(OTHER_TEST_CHANNEL, CallTestHandlerFunc2);
            Assert.IsFalse(EventHubCore.Call<RedisPubSubChannel, int, bool>(OTHER_TEST_CHANNEL, 1));
        }


        private Response<bool> CallTestHandlerFunc(Request<int, bool> request)
        {
            var response = request.MyResponse;
            response.ResponseContent = request.RequestContent == 0 ? true : false;
            return response;
        }

        private Response<bool> CallTestHandlerFunc2(Request<int, bool> request)
        {
            var response = request.MyResponse;
            response.ResponseContent = request.RequestContent != 1 ? true : false;
            return response;
        }

        private void SendTestHandler(Request<int, bool> request)
        {
            _semaphore.Release();
        }

        private void SendTestHandler2(Request<int, bool> request)
        {
            _semaphore2.Release();
        }

        private void SendTestHandler3(Response<bool> response)
        {
            _semaphore.Release();
        }

        private void SendTestHandler4(TestType message)
        {
            _semaphore.Release();
        }

        private SemaphoreSlim _semaphore = new SemaphoreSlim(0, 1);

        private SemaphoreSlim _semaphore2 = new SemaphoreSlim(0, 1);

        private const string TEST_CHANNEL = "testchannel";
        private const string OTHER_TEST_CHANNEL = "other testchannel";
    }

    public class TestType
    {
        public string Id { get; set; } 
    }
}
