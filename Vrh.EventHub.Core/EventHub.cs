using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vrh.EventHub.Core.Interfaces;
using Vrh.Logger;

namespace Vrh.EventHub.Core
{
    /// <summary>
    /// Interprocess esményközvetítő szolgáltatás
    /// </summary>
    public static class EventHub
    {
        static EventHub()
        {
            Logger.Logger.Log("Loading Event Hub... ", LogLevel.Information, typeof(EventHub));
        }            

        /// <summary>
        /// A kért emittert adja
        /// </summary>
        /// <typeparam name="TChannel">Csatorna típusa</typeparam>
        /// <typeparam name="TContract">Szerződés, amit az Emitter ismer</typeparam>
        /// <param name="emitterId">Az emitter példány azonosítója</param>
        /// <returns></returns>
        public static IEmitter GetEmitter<TChannel, TContract>(string emitterId)
                                            where TChannel : BaseChannel, new()
                                            where TContract : BaseContract, new()
        {
            IChannel channel = GetChannel<TChannel>();
            return channel.GetEmitter<TContract>(emitterId);
        }

        /// <summary>
        /// A kért collectort adja
        /// </summary>
        /// <typeparam name="TChannel">Csatorna típusa</typeparam>
        /// <typeparam name="TContract">Szerződés, amit az Emitter ismer</typeparam>
        /// <param name="collectorId">A collector példány azonosítója</param>0
        /// <returns></returns>
        public static ICollector GetCollector<TChannel, TContract>(string collectorId) 
                                                where TChannel : BaseChannel, new()
                                                where TContract : BaseContract, new()
        {
            IChannel channel = GetChannel<TChannel>();
            return channel.GetCollector<TContract>(collectorId);
        }

        /// <summary>
        /// Event hub core konfiguráció
        /// </summary>        
        internal static EventHubCoreConfig EventHubCoreConfiguration
        {
            get { return _eventHubCoreConfiguration.Value; }
        }

        /// <summary>
        /// A megadott típusú csatornát adja, ha bnincs még ilyen, létrehozza 
        /// </summary>
        /// <typeparam name="TChannel"></typeparam>
        /// <returns></returns>
        private static IChannel GetChannel<TChannel>()
                                                where TChannel : BaseChannel, new()
        {
            lock (_existingChannel)
            {
                IChannel channel = _existingChannel.FirstOrDefault(x => x.ChannelType ==
                                                                           typeof(TChannel).FullName);
                if (channel == null)
                {
                    channel = new TChannel();
                    channel.RegisterMe();
                    _existingChannel.Add(channel);
                }
                return channel;
            }
        }

        /// <summary>
        /// A létező csatornák listája
        /// </summary>
        private static List<IChannel> _existingChannel = new List<IChannel>();

        /// <summary>
        /// Felépíti az EventHubCoreConfig osztályt
        /// </summary>
        /// <returns></returns>
        private static EventHubCoreConfig ConfigFactory()
        {
            string config = ConfigurationManager.AppSettings[$"{EventHubCoreConfig.MODUL_PREFIX}:Config"];
            if (String.IsNullOrEmpty(config))
            {
                config = "EventHub.Core.Config.xml/Vrh.EventHub.Core";
            }
            Logger.Logger.Log($"Used EventHub configuration: {config}", LogLevel.Information, typeof(EventHub));
            return new EventHubCoreConfig(config);
        }

        /// <summary>
        /// EventHub core beállításai
        /// </summary>
        private static readonly Lazy<EventHubCoreConfig> _eventHubCoreConfiguration = new Lazy<EventHubCoreConfig>(new Func<EventHubCoreConfig>(ConfigFactory));
    }
}
    