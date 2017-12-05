using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vrh.EventHub.Core.Interfaces
{
    public interface ICollector
    {
        /// <summary>
        /// A collector típusa
        /// </summary>
        string CollectorType { get; }

        /// <summary>
        /// A collector azonosítója
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// A collector emberibb leírása
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Registrálja az adott típusú üzenet feldolgozó függvényét
        /// </summary>
        /// <typeparam name="TMessage">Üzenet típus</typeparam>
        /// <param name="handler">az üzenetet kezelő függvény</param>
        void RegisterMessageHandler<TMessage>(Func<IMessage, IMessage> handler) where TMessage : BaseMessage, new();
    }
}
