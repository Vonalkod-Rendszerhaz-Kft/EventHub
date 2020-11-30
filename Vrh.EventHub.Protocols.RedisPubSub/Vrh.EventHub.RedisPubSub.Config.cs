using System;
using System.Configuration;
using Vrh.XmlProcessing;

namespace Vrh.EventHub.Protocols.RedisPubSub
{
    internal class EventHubRedisPubSubConfig : LinqXMLProcessorBase
    {
        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="parameterFile">XML fájl aminek a feldolgozására az osztály készül</param>
        internal EventHubRedisPubSubConfig(string parameterFile):base(parameterFile) { }
		/// <summary>
		/// Constructor 
		/// </summary>
		/// <param name="parameterFile">XML fájl aminek a feldolgozására az osztály készül</param>
		/// <param name="redisconnectionstring">XML fájl aminek a feldolgozására az osztály készül</param>
		/// <param name="timeout">XML fájl aminek a feldolgozására az osztály készül</param>
		internal EventHubRedisPubSubConfig(string parameterFile, string redisconnectionstring=null, TimeSpan? timeout=null) : base(parameterFile)
		{
			_timeout = timeout;
			_redisconnectionstring = redisconnectionstring;
		}

		private string _redisconnectionstring = null;
		private TimeSpan? _timeout = null;
		#region Retrive information from XML

		/// <summary>
		/// A modul által használt redis connection alias
		/// </summary>
		internal string RedisConnection
        {
            get
            {
                string redisConnection = ConfigurationManager.AppSettings[RedisPubSubChannel.MODUL_PREFIX + ":" + Me.RedisConnectionAliasElement.NAME];
				if (String.IsNullOrEmpty(redisConnection)) { redisConnection = _redisconnectionstring; }
				if (String.IsNullOrEmpty(redisConnection)) { redisConnection = GetValue(GetXElement(Me.RedisConnectionAliasElement.NAME), "localhost"); }
				try { redisConnection = Vrh.XmlProcessing.ConnectionStringStore.GetRedisCM(redisConnection); } catch { }
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
				TimeSpan timeout;
				if (timoutStr == null) { timeout = (TimeSpan)_timeout; }
				else if (!TimeSpan.TryParse(timoutStr, out timeout)) { timeout = GetValue<TimeSpan>(GetXElement(Me.ChannelTimoutElement.NAME), new TimeSpan(0, 0, 1)); }
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
