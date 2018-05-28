using System;
using System.Configuration;
using Vrh.LinqXMLProcessor.Base;

namespace Vrh.EventHub.Protocols.RedisPubSub
{
    internal class EventHubRedisPubSubConfig : LinqXMLProcessorBaseClass
    {
        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="parameterFile">XML fájl aminek a feldolgozására az osztály készül</param>
        internal EventHubRedisPubSubConfig(string parameterFile)
        {
            _xmlFileDefinition = parameterFile;
        }

        #region Retrive information from XML

        /// <summary>
        /// A modul által használt redis connection alias
        /// </summary>
        internal string RedisConnection
        {
            get
            {
                string redisConnection = ConfigurationManager.AppSettings[RedisPubSubChannel.MODUL_PREFIX + ":" + Me.RedisConnectionAliasElement.NAME];
                if (String.IsNullOrEmpty(redisConnection))
                {
                    redisConnection = GetElementValue(GetXElement(Me.RedisConnectionAliasElement.NAME), "localhost");
                }
                return redisConnection;
            }
        }

        /// <summary>
        /// A csatornán konfigurált általános timout
        /// </summary>
        internal TimeSpan ChannelTimeout
        {
            get
            {
                string timoutStr = ConfigurationManager.AppSettings[RedisPubSubChannel.MODUL_PREFIX + ":" + Me.ChannelTimoutElement.NAME];
                TimeSpan timeout = new TimeSpan(0, 0, 1);
                if(!TimeSpan.TryParse(timoutStr, out timeout))
                {
                    timeout = GetElementValue<TimeSpan>(GetXElement(Me.ChannelTimoutElement.NAME), new TimeSpan(0, 0, 1));
                }
                return timeout;
            }
        }

        #endregion Retrive information from XML

        #region Defination of namming rules in XML

        /// <summary>
        /// A konfigurációt tartalmazó XML struktura leírója
        /// </summary>
        internal static class Me
        {
            /// <summary>
            /// A használt redis connection alias
            /// </summary>
            internal static class RedisConnectionAliasElement
            {
                /// <summary>
                /// A használt redis connection alias: Az XML tag neve 
                /// </summary>
                internal const string NAME = "RedisConnection";
            }
            /// <summary>
            /// A csatornán konfigurált timeout
            /// </summary>
            internal static class ChannelTimoutElement
            {
                /// <summary>
                /// A csatornán konfigurált timeout: Az XML tag neve
                /// </summary>
                internal const string NAME = "ChannelTimeout";
            }
        }

        #endregion Defination of namming rules in XML
    }
}
