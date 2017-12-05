using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vrh.EventHub.Core.Exceptions;

namespace Vrh.EventHub.Core.Interfaces
{
    /// <summary>
    /// A Collectorok ősosztálya, minden Collectort ebből kell származtatni!
    /// </summary>
    public class BaseCollector : ICollector
    {
        /// <summary>
        /// Visszadja a Collector típusát
        /// </summary>
        public string CollectorType
        {
            get
            {
                return this.GetType().FullName;
            }
        }

        /// <summary>
        /// Collector azonosítója
        /// </summary>
        public string Id
        {
            get
            {
                return _id;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    EventHubException.ThrowOrNo(new EmptyCollectorIdNotPosibleException(), this.GetType());
                }
                if (_id != null)
                {
                    EventHubException.ThrowOrNo(new CollectorIdAlreadySettedException(), this.GetType());
                }
                _id = value;
            }
        }

        /// <summary>
        /// A Collector típushoz tartozó emberi fogyasztásra szánt információ
        /// </summary>
        public string Description
        {
            get
            {
                return _description;
            }
        }

        /// <summary>
        /// Registrálja az adott típusú üzenet feldolgozó függvényét
        /// </summary>
        /// <typeparam name="TMessage">Üzenet típus</typeparam>
        /// <param name="handler">az üzenetet kezelő függvény</param>
        public void RegisterMessageHandler<TMessage>(Func<IMessage, IMessage> handler) where TMessage : BaseMessage, new()
        {
            Type msgType = typeof(TMessage);
            HandlerWithType hwt = _handlers.FirstOrDefault(x => x.MessageType == msgType);
            if(hwt == null)
            {
                hwt = new HandlerWithType();
                hwt.MessageType = msgType;
                _handlers.Add(hwt);
            }
            hwt.Handler = handler;
        }

        /// <summary>
        /// Visszadaja a collectoron regisztrált eseményfeldolgozók (handlerek) listáját
        /// </summary>
        internal List<HandlerWithType> Handlers
        {
            get
            {
                return _handlers;
            }
        }

        /// <summary>
        /// A collector azonosítója
        /// </summary>
        protected string _id = null;

        /// <summary>
        /// A collector típushoz tartozó emberi fogyasztásra szánt információ (Adj neki értéket a leszármazottban!)
        /// </summary>
        protected string _description = null;

        /// <summary>
        /// az üzenetek kezelői
        /// </summary>
        protected List<HandlerWithType> _handlers = new List<HandlerWithType>();
    }

    /// <summary>
    /// Az üzenet kezelőket (Handler) és az általuk kezelt üzenet párok katalógusa  
    /// </summary>
    public class HandlerWithType
    {
        /// <summary>
        /// Üzenet típus, melyet ez a handler kezel
        /// </summary>
        public Type MessageType { get; set; }

        /// <summary>
        /// A kezelést végzó függvény (ezt hívja, ha ilyen üzenet jön)
        /// </summary>
        public Func<IMessage, IMessage> Handler { get; set; }
    }
}
