using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vrh.EventHub.Core;
using StackExchange.Redis;
using Newtonsoft.Json;
using System.Configuration;
using Vrh.Logger;
using System.Threading;

namespace Vrh.EventHub.Protocols.RedisPubSub
{
    /// <summary>
    /// Redis Pub/Sub szolgáltatás felett működő EventHub csatorna
    /// Alaklamas alakalmazás közti kommunikációra
    /// </summary>
    public class RedisPubSubChannel : BaseChannel
    {
        /// <summary>
        /// Megmondja, hogy megvan-er  akapcsolat a Redis kiszolgáló felé
        /// </summary>
        /// <returns></returns>
        static bool RedisPubSubChannelConnected()
        {
            return Connection.IsConnected;            
        }

        /// <summary>
        /// Send (Aszinkron) üzenetküldés csatornaimplementáció
        /// </summary>
        /// <param name="message">Elküldendő üzenet</param>
        public override void Send(EventHubMessage message)
        {
            var th = new Thread(() => PubSub.Publish(ChannelId, JsonConvert.SerializeObject(message), CommandFlags.HighPriority))
            {
                Name = "SendThread",
                Priority = ThreadPriority.Highest,
                IsBackground = true
            };
            th.Start();
        }

        private static readonly Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() => {
            var configfile = new EventHubRedisPubSubConfig(GetConfigFile());
            return ConnectionMultiplexer.Connect(configfile.RedisConnection);
        });

        /// <summary>
        /// Redis connection (shared mintának megfelelő implementáció)
        /// </summary>
        public static ConnectionMultiplexer Connection
        {
            get
            {
                return lazyConnection.Value;
            }
        }

        /// <summary>
        /// Inicializálja a csatorna infrastruktúráját
        /// </summary>
        public override void InitializeChannelInfrastructure()
        {
            RedisPubSubChannelConnected();
            PubSub.Subscribe(ChannelId, HandleInputMessages, CommandFlags.HighPriority);
            base.InitializeChannelInfrastructure();
        }

        /// <summary>
        /// Redis csatorna konfigurált timeoutja
        /// </summary>
        /// <returns></returns>
        public override TimeSpan GetChannelTimout()
        {
            return Configuration.ChannelTimeout;
        }

        /// <summary>
        /// Üzenet érkezik  acsatornára
        /// </summary>
        /// <param name="channel">csatorna szonosító 
        ///     (redis oldalnak kell (illetve mindig visszaküldi a csatorna id-t is), 
        ///         mivel csatornánként külön subscriberünk van, ezt nem használjuk ki, mindig ChannelId-val egyezik)</param>
        /// <param name="message">Érkezett üzenet (json serializált )</param>
        private void HandleInputMessages(RedisChannel channel, RedisValue message)
        {
            EventHubMessage msg = JsonConvert.DeserializeObject<EventHubMessage>(message);
            var th = new Thread(() => _coreInputMessageHandler.Invoke(msg))
            {
                Name = "HandleInputMessage Thread",
                Priority = ThreadPriority.Highest,
                IsBackground = true
            };
            th.Start();
        }

        /// <summary>
        /// A csatorna konfigurációja
        /// </summary>
        internal EventHubRedisPubSubConfig Configuration
        {
            get
            {
                lock (_getConfigurationLocker)
                {
                    if (_configuration == null)
                    {
                        _configuration = new EventHubRedisPubSubConfig(ConfigFile);
                    }
                    return _configuration;
                }
            }
        }

        /// <summary>
        /// Visszaadja a csatornához tartozó Redis PubSub objektumot
        ///  Ha nincs még, akkor beszerzi
        ///  Ha disposol a csatorna, akkor Exceptiont ad
        /// </summary>
        private ISubscriber PubSub
        {
            get
            {
                if (disposedValue)
                {
                    EventHubException.ThrowOrNo(
                        new FatalEventHubException("This channel is under unloading and not usable!"),
                        this.GetType(),
                        new Dictionary<string, string>()
                        {
                            {"Channel id", ChannelId},
                        });
                }
                lock (_getPubSubLocker)
                {
                    if (_pubsub == null)
                    {
                        var cm = Connection;
                        _pubsub = cm.GetSubscriber();
                    }
                    return _pubsub;
                }
            }
        }

        /// <summary>
        /// Lockolja a pub/sub elkérést, hogy mindenki garantáltan uyganazzal az objektummal dolgozon
        /// </summary>
        private readonly object _getPubSubLocker = new object();

        /// <summary>
        /// Lockolja a Configuration elérést, hogy mindenki garantáltan ugyanazzal a referenciával dolgozon
        /// </summary>
        private readonly object _getConfigurationLocker = new object();

        /// <summary>
        /// A konfiguráció (XMLProcesszor)
        /// </summary>
        private EventHubRedisPubSubConfig _configuration = null;

        /// <summary>
        /// a csatornához tartozó StackExchange.Redis PubSub objektum
        /// </summary>
        private ISubscriber _pubsub = null;

        /// <summary>
        /// Visszadja, hogy mi a RedisPubSub csatorna konfigurációs fájlja  
        /// </summary>
        private string ConfigFile
        {
            get
            {
                string config = GetConfigFile();
                VrhLogger.Log(
                    $"Used EventHub.RedisPubSub channel configuration: {config}",
                    new Dictionary<string, string>()
                    {
                        {"Channel id", ChannelId},
                        {"Configuration", config },
                    },
                    null,
                    LogLevel.Information,
                    this.GetType());
                return config;
            }
        }

        /// <summary>
        /// Visszaadja a használt config fájl nevét
        /// </summary>
        /// <returns>Config fájl név</returns>
        private static string GetConfigFile()
        {
            string config = ConfigurationManager.AppSettings[$"{MODUL_PREFIX}:Config"];
            if (string.IsNullOrEmpty(config))
            {
                config = "Vrh.EventHub.RedisPubSub.Config.xml/Vrh.EventHub.RedisPubSub";
            }
            return config;
        }

        #region IDisposable Support
        /// <summary>
        /// Part of Dispose pattern
        /// </summary>
        /// <param name="disposing">más dispos-ol?</param>
        protected override void Dispose(bool disposing)
        {
            PubSub.Unsubscribe(ChannelId);
            base.Dispose(disposing);
            _configuration.Dispose();
        }

        #endregion IDisposable Support

        /// <summary>
        /// A modul egyedi prefixe
        /// </summary>
        internal const string MODUL_PREFIX = "Vrh.EventHub.RedisPubSub";
    }
}
