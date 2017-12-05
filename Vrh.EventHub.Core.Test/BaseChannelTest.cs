using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vrh.EventHub.Core.Interfaces;
using Vrh.EventHub.Core;
using System.Linq;

namespace Vrh.EventHub.Core.Test
{
    [TestClass]
    public class BaseChannelTest
    {
        [TestMethod]
        public void BaseChannel_ChannelType()
        {
            IChannel ch = new BaseChannel();
            Assert.AreEqual(typeof(BaseChannel).FullName, ch.ChannelType);
        }

        [TestMethod]
        public void BaseChannel_Dispose()
        {
            var d = typeof(BaseChannel).GetMethod("Dispose");
            Assert.IsTrue(d.IsVirtual);
            IChannel ch = new BaseChannel();            
            bool notImplemented = false;
            try
            {
                ch.Dispose();
            }
            catch(NotImplementedException)
            {
                notImplemented = true;
            }
            Assert.IsTrue(notImplemented);
        }

        [TestMethod]
        public void BaseChannel_RegisterMe()
        {
            var d = typeof(BaseChannel).GetMethod("RegisterMe");
            Assert.IsTrue(d.IsVirtual);
            IChannel ch = new BaseChannel();
            bool notImplemented = false;
            try
            {
                ch.RegisterMe();
            }
            catch (NotImplementedException)
            {
                notImplemented = true;
            }
            Assert.IsTrue(notImplemented);
        }

        [TestMethod]
        public void BaseChannel_GetEmitter()
        {
            string emitterId = typeof(TestContract).FullName;
            IChannel ch = new BaseChannel();
            IEmitter e = ch.GetEmitter<TestContract>(emitterId);
            KnownContractsWithInfrastucture c = ((BaseChannel)ch).KnownContracts.FirstOrDefault(x => x.Contract.Id == typeof(TestContract).FullName);
            Assert.AreNotEqual(null, c);
            IEmitter e2 = c.Emitters.FirstOrDefault(x => x.Id == emitterId);
            Assert.AreNotEqual(null, e2);
            Assert.AreSame(e, e2);
            IEmitter e3 = ch.GetEmitter<TestContract>(emitterId);
            Assert.AreSame(e, e3); 
        }

        [TestMethod]
        public void BaseChannel_GetCollector()
        {
            string collectorId = typeof(TestContract).FullName;
            IChannel ch = new BaseChannel();
            ICollector c1 = ch.GetCollector<TestContract>(collectorId);
            KnownContractsWithInfrastucture kc = ((BaseChannel)ch).KnownContracts.FirstOrDefault(x => x.Contract.Id == typeof(TestContract).FullName);
            Assert.AreNotEqual(null, kc);
            ICollector c2 = kc.Collectors.FirstOrDefault(x => x.Id == collectorId);
            Assert.AreNotEqual(null, c2);
            Assert.AreSame(c1, c2);
            ICollector c3 = ch.GetCollector<TestContract>(collectorId);
            Assert.AreSame(c1, c3);
        }

    }
}