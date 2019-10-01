using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vrh.EventHub.Intervention
{
    /// <summary>
    /// Az egyszerűsített "beavatkozás" interfész EventHub szerződése
    /// </summary>
    internal static class InterventionContract
    {
        /// <summary>
        /// Beavatkozás Kérés
        /// </summary>
        internal class InterventionRequest
        {
            public string InterventionName { get; set; }

            public Dictionary<string, string> Parameters { get; set; }

            public string ChannelId { get; set; }
        }

        /// <summary>
        /// Beavatkozás válasz
        /// </summary>
        internal class InterventionResponse
        {
            public Dictionary<string, string> Data { get; set; }
        }
    }
}
