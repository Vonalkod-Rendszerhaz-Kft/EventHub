using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vrh.EventHub.Core.Interfaces;

namespace Vrh.EventHub.Core
{
    /// <summary>
    /// Alapértelmezett callback. Ezt használja olyan message-ek visszatérésére, melyek nem tartalmaznak callbacket. 
    /// </summary>
    internal class DefaultCallbackMessage : BaseMessage
    {
        /// <summary>
        /// OK/NOK
        /// </summary>
        public bool IsOK { get; set; } = true;

        /// <summary>
        /// Üzenet
        /// </summary>
        public string Message { get; set; } = "Send message: OK"; 
    }
}
