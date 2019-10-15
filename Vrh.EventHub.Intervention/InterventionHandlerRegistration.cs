using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vrh.EventHub.Intervention
{
    /// <summary>
    /// Beavatkozás kezelő regisztrációk tárolására szolgáló osztály
    /// </summary>
    internal class InterventionHandlerRegistration
    {
        /// <summary>
        /// Csatorna azonosító
        /// </summary>
        public string ChannelId { get; set; }

        /// <summary>
        /// Kezelő metódus (handler) referencia 
        /// </summary>
        public Func<string, Dictionary<string, string>, Dictionary<string, string>> Handler { get; set; }
    }
}
