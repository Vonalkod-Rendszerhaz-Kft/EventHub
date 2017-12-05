using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Vrh.Logger;

namespace Vrh.EventHub.Core.Exceptions
{
    /// <summary>
    /// EventHub exception base class (A beállításoktól függőne lenyeli, vagy dobja az ilyen exception-öket)
    /// </summary>
    public class EventHubException : Exception
    {   
        /// <summary>
        /// A beállításokanak megfelelően exceptiont dob, vagy logolja  ahibát
        /// </summary>
        /// <param name="ex"></param>
        public static void ThrowOrNo(EventHubException ex, Type source, [CallerMemberName]string caller = "", [CallerLineNumber]int line = 0)
        {              
            if (ex is FatalEventHubException)
            {
                Logger.Logger.Log(ex, source, LogLevel.Fatal, caller, line);
                throw ex;
            }
            else
            {
                Logger.Logger.Log(ex, source, LogLevel.Error, caller, line);
                if (EventHub.EventHubCoreConfiguration.ThrowEventHubCoreExceptions)
                {
                    throw ex;
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Exception message</param>
        public EventHubException(string message) : base(message)
        {
        }
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
        public FatalEventHubException(string message) : base($"Fatal error: {message}")
        {
        }
    }
}
