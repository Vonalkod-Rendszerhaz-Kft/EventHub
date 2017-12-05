using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vrh.EventHub.Core.Exceptions
{
    /// <summary>
    /// Üres Emitter id-t jelző kivétel
    /// </summary>
    public class EmptyEmitterIdNotPosibleException : FatalEventHubException
    {
        public EmptyEmitterIdNotPosibleException() : base("Empty emitter id is not posibble!")
        {
        }
    }

    /// <summary>
    /// Ez az exception akkor jelentkezik, ha megkisérlünk 1-nél többször ID-t beállítani egy collectoron  
    /// </summary>
    public class EmitterIdAlreadySettedException : FatalEventHubException
    {
        public EmitterIdAlreadySettedException() : base("Emitter id already setted! Emitter id is not overwritable!")
        {
        }
    }
}
