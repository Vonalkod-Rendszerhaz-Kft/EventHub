using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vrh.EventHub.Core.Interfaces;

namespace Vrh.EventHub.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class MessageEventArgs : EventArgs
    {
        /// <summary>
        /// Konstruktor
        /// </summary>
        /// <param name="message"></param>
        public MessageEventArgs(Interfaces.IMessage message)
        {
            _message = message;
        }

        /// <summary>
        /// Üzenet elérése
        /// </summary>
        public Type Message { get; }

        /// <summary>
        /// Üzenet
        /// </summary>
        private Interfaces.IMessage _message;
    }
}
