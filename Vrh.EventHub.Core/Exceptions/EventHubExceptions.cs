using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vrh.Logger;

namespace Vrh.EventHub.Core
{
	#region GenericEventHubException Exception
	/// <summary>
	/// Osztály az alkalmazásban az EventHub Response-ba elhelyezendő exception-öknek
	/// </summary>
	[Serializable]
	public class EventHubResponseException : Exception
	{
		public EventHubResponseException() : base(){}
		public EventHubResponseException(string message) : base(message){}
		public EventHubResponseException(string message, Exception ex) : base(message, ex){}
		public EventHubResponseException(System.Runtime.Serialization.SerializationInfo serinfo, System.Runtime.Serialization.StreamingContext serctx) : base(serinfo, serctx) { }
	}
	#endregion GenericEventHubException Exception

	/// <summary>
	/// EventHub exception base class (A beállításoktól függőne lenyeli, vagy dobja az ilyen exception-öket)
	/// </summary>
	public class EventHubException : EventHubResponseException
	{
        /// <summary>
        /// A beállításokanak megfelelően exceptiont dob, vagy logolja  ahibát
        /// </summary>
        /// <param name="ex">Kivétel</param>
        /// <param name="source">forrás típus</param>
        /// <param name="data"></param>
        /// <param name="warningOnly">Csak warningként logolja</param>
        /// <param name="caller">hívó metódus</param>
        /// <param name="line">hivás sora</param>
        public static void ThrowOrNo(EventHubException ex, Type source, Dictionary<string, string> data = null, bool warningOnly = false, [CallerMemberName]string caller = "", [CallerLineNumber]int line = 0)
        {            
            if (ex is FatalEventHubException)
            {
                VrhLogger.Log("Eventhub Exception!", data, ex, LogLevel.Fatal, source, caller, line);
                throw ex;
            }
            else
            {
                var level = warningOnly ? LogLevel.Warning : LogLevel.Error;
                VrhLogger.Log("Eventhub Exception!", data, ex, LogLevel.Fatal, source, caller, line);
                if (EventHubCore.EventHubCoreConfiguration.ThrowEventHubCoreExceptions)
                {
                    throw ex;
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Exception message</param>
        public EventHubException(string message) : base($"{EVENTHUB_ERROR_PREFIX}{message}")
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">hibaüzenet</param>
        /// <param name="innerExcception">eredeti kivétel</param>
        public EventHubException(string message, Exception innerExcception) : base($"{EVENTHUB_ERROR_PREFIX}{message}", innerExcception)
        {
        }

        internal const string EVENTHUB_ERROR_PREFIX = "EventHub error: ";
    }

    /// <summary>
    /// Kritikus event Hub exception (ezt mindig továbbdobja)
    /// </summary>
    public class FatalEventHubException : EventHubException
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">hibaüzenet</param>
        public FatalEventHubException(string message) : base(message)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Hibaüzenet</param>
        /// <param name="innerException">eredeti kivétel</param>
        public FatalEventHubException(string message, Exception innerException) : base(message, innerException)
        {
        }        
    }
}
