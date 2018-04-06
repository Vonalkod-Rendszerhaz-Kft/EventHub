using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vrh.EventHub.Core;
using Vrh.Logger;

namespace Vrh.EventHub.Protocols.InsideApplication
{

    public class InsideApplicationChannel : BaseChannel
    {
        /// <summary>
        /// Send (Aszinkron) üzenetküldés csatornaimplementáció
        /// </summary>
        /// <typeparam name="TMessage">A küldött üzenet</typeparam>
        /// <param name="message">üzenet</param>
        public override void Send(EventHubMessage message)
        {
            Task.Run(() => HandleInputMessages(message));
        }

        /// <summary>
        /// Inicializálja a csatorna infrastruktúráját
        /// </summary>
        public override void InitializeChannelInfrastructure()
        {
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
        /// A csatorna konfigurációja
        /// </summary>
        internal EventHubInsideApplicationConfig Configuration
        {
            get
            {
                lock (_getConfigurationLocker)
                {
                    if (_configuration == null)
                    {
                        _configuration = new EventHubInsideApplicationConfig(ConfigFile);
                    }
                    return _configuration;
                }
            }
        }

        /// <summary>
        /// A konfiguráció (XMLProcesszor)
        /// </summary>
        private EventHubInsideApplicationConfig _configuration = null;

        /// <summary>
        /// Visszadja, hogy mi a RedisPubSub csatorna konfigurációs fájlja  
        /// </summary>
        private string ConfigFile
        {
            get
            {
                string config = ConfigurationManager.AppSettings[$"{MODUL_PREFIX}:Config"];
                if (String.IsNullOrEmpty(config))
                {
                    config = "Vrh.EventHub.InsideApplication.Config.xml/Vrh.EventHub.InsideApplication";
                }
                VrhLogger.Log(
                    $"Used EventHub.InsideApplication channel configuration: {config}",
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
        /// Lockolja a Configuration elérést, hogy mindenki garantáltan ugyanazzal a referenciával dolgozon
        /// </summary>
        private readonly object _getConfigurationLocker = new object();

        /// <summary>
        /// Üzenet érkezik  acsatornára
        /// </summary>
        /// <param name="channel">csatorna szonosító 
        ///     (redis oldalnak kell (illetve mindig visszaküldi a csatorna id-t is), 
        ///         mivel csatornánként külön subscriberünk van, ezt nem használjuk ki, mindig ChannelId-val egyezik)</param>
        /// <param name="message">Érkezett üzenet (json serializált )</param>
        private void HandleInputMessages(EventHubMessage message)
        {
            // csak átpaszolja (aszinkron) az érkező üzenetet a Core-nak
            //Task.Run(() => _coreInputMessageHandler.DynamicInvoke(new object[] { message }));
            Task.Run(() => _coreInputMessageHandler.Invoke(message));
        }

        #region IDisposable Support
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        #endregion IDisposable Support

        /// <summary>
        /// A modul egyedi prefixe
        /// </summary>
        internal const string MODUL_PREFIX = "Vrh.EventHub.InsideApplication";
    }
}
