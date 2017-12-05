using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vrh.EventHub.Core.Exceptions
{
    /// <summary>
    /// Üres collector id-t jelző kivétel
    /// </summary>
    public class EmptyCollectorIdNotPosibleException : FatalEventHubException
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public EmptyCollectorIdNotPosibleException() : base("Empty commiter id is not posibble!")
        {
        }
    }

    /// <summary>
    /// Ez az exception akkor jelentkezik, ha megkisérlünk 1-nél többször ID-t beállítani egy collectoron  
    /// </summary>
    public class CollectorIdAlreadySettedException : FatalEventHubException
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CollectorIdAlreadySettedException() : base("Collector id already setted! Emitter id is not overwritable!")
        {
        }
    }
}
