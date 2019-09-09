using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Vrh.Logger;
using VRH.Common;

namespace Vrh.EventHub.Core
{
    /// <summary>
    /// Tipusvezérelt esményközvetítő szolgáltatás, mely tetszőleges csatornaimplementáció felett tud működni
    /// </summary>
    public static partial class EventHubCore
    {
        /// <summary>
        /// Static constructor
        /// </summary>
        static EventHubCore()
        {
            var logData = new Dictionary<string, string>
            {
                { "EventHub endpoint", Assembly.GetEntryAssembly().CodeBase },
                { "Endpoint fullname", Assembly.GetEntryAssembly().FullName },
                { "EventHub assembly", Assembly.GetExecutingAssembly().CodeBase },
                { "EventHub core version", Assembly.GetExecutingAssembly().Version() }
            };
            VrhLogger.Log("Loading Event Hub... ", logData, null, LogLevel.Verbose, typeof(EventHubCore));
        }

        /// <summary>
        /// Inicializálja a megadott csatornát
        ///     Segítségével kiemelhető a csatorna infrastruktúra kreállási költslége ami így:
        ///         - nem az első tényleges művekletet fogja terhelni
        ///         - Redukáklja az erősen túlterhelt párhuzamos környezetben való működést terhelő problémákat 
        ///             (mint pl. a redis pub/sub feliratkozásoknál feléppő timeoutok) 
        /// </summary>
        /// <typeparam name="TChannel">A csatorna típusa</typeparam>
        /// <param name="channelId">csatorna azonosító</param>
        public static void InitielizeChannel<TChannel>(string channelId)
            where TChannel : BaseChannel, new()
        {
            var registeredChannel =
                _channels.FirstOrDefault(x => x.Id == channelId && x.Channel.GetType() == typeof(TChannel))
                ?? RegisterChannel<TChannel>(channelId);
        }

        /// <summary>
        /// Aszinkron üzenetküldés!
        ///     Olyan üzenetek elküldésére használandó, amelyre a küldő oldal semmiylen módon nem vár választ. 
        /// A küldés során keletkező exceptiön-ök (Pl. közvetitő infrastruktúra hibái) felmennek a hívási helyre. 
        /// </summary>
        /// <typeparam name="TChannel">Csatorna típusa</typeparam>
        /// <typeparam name="TMessage">Üzenet típusa</typeparam>
        /// <param name="channelId">Csatorna azonosító</param>
        /// <param name="message">üzenet</param>
        public static void Send<TChannel, TMessage>(string channelId, TMessage message)
            where TMessage : new()
            where TChannel : BaseChannel, new()
        {
            EventHubSend<TChannel, TMessage>(channelId, message);
        }

        /// <summary>
        /// Teljesen aszinkron üzenetküldés, amely során az átviteli hibák is elnyelődnek!!! 
        ///     Cserébe nagyon magas párhuzamos teljesítményre képes! És a hívás oldali költsége elhanyagolható.
        ///     Olyan üzenetek elküldésére használandó, amelyre a küldő oldal semmiylen módon nem vár választ.
        ///     Beleértev azt, is, hogy nem akar róla tudni, hogy maga  aküldés sikeres-e.
        ///     Pl.: logolásra, monitoring kliensek müködtetésére használható fel jól.
        ///     A küldés során keletkező exceptiön-ök (Pl. közvetitő infrastruktúra hibái) 
        ///     mindig elvesznek a hivó szemszögéből. 
        /// </summary>
        /// <typeparam name="TChannel">Csatorna típusa</typeparam>
        /// <typeparam name="TMessage">Üzenet típusa</typeparam>
        /// <param name="channelId">Csatorna azonosító</param>
        /// <param name="message">üzenet</param>
        public static void SendAsync<TChannel, TMessage>(string channelId, TMessage message)
            where TMessage : new()
            where TChannel : BaseChannel, new()
        {
            Task.Run(() => EventHubSend<TChannel, TMessage>(channelId, message));
        }

        /// <summary>
        /// Aszinkron Request üzenetküldés: Kérés oldal -> Elküld egy Request-et 
        /// A küldés során keletkező exceptiön-ök (Pl. közvetitő infrastruktúra hibái) az EventHub 
        ///     keret ThrowException beállításától függően mennek vagy nem mennek fel a hívási helyre. 
        /// </summary>
        /// <typeparam name="TChannel">Csatorna implementáció típusa</typeparam>
        /// <typeparam name="TRequest">Kérés típusa</typeparam>
        /// <typeparam name="TResponse">Kérés típusa</typeparam>        
        /// <param name="channelId">Csatorna azonosító, ahová küldi</param>
        /// <param name="request">Kérés üzenet</param>
        public static void Send<TChannel, TRequest, TResponse>(string channelId, Request<TRequest, TResponse> request)
            where TRequest : new()
            where TResponse : new()
            where TChannel : BaseChannel, new()
        {
            EventHubSend<TChannel, Request<TRequest, TResponse>>(channelId, request);
        }

        /// <summary>
        /// Aszinkron Response üzenetküldés: Válasz oldal -> Elküld egy Response-t 
        /// A küldés során keletkező exceptiön-ök (Pl. közvetitő infrastruktúra hibái) felmennek a hívási helyre. 
        /// </summary>
        /// <typeparam name="TChannel">Csatorna implementáció típusa</typeparam>
        /// <typeparam name="TResponse">Kérés típusa</typeparam>        
        /// <param name="channelId">Csatorna azonosító, ahová küldi</param>
        /// <param name="response">Kérés üzenet</param>
        public static void Send<TChannel, TResponse>(string channelId, Response<TResponse> response)
            where TResponse : new()
            where TChannel : BaseChannel, new()
        {
            EventHubSend<TChannel, Response<TResponse>>(channelId, response);
        }

        /// <summary>
        /// Szinkron üzenetküldés!!! Mindig megvárja a választ!!! 
        /// A küldés során keletkező exceptiön-ök (Pl. közvetitő infrastruktúra hibái) felmennek a hívási helyre. 
        /// </summary>
        /// <typeparam name="TChannel">Csatorna implementáció típusa</typeparam>
        /// <typeparam name="TRequest">Küldendő kérés típus</typeparam>
        /// <typeparam name="TResponse">Várt válasz típusa</typeparam>
        /// <param name="channelId">Csatorna azonosító</param>
        /// <param name="request">Kérés példány</param>
        /// <param name="timeOut">Használt timout, ha az EventHub konfigurált alapértelmezésétől eltérjen</param>
        /// <returns>A hívás visszatérési típusa szerinti objektumpéldány</returns>        
        public static TResponse Call<TChannel, TRequest, TResponse>(string channelId, TRequest request, TimeSpan? timeOut = null)
            where TRequest : new()
            where TResponse : new()
            where TChannel : BaseChannel, new()
        {
            var registeredChannel =
                _channels.FirstOrDefault(x => x.Id == channelId && x.Channel.GetType() == typeof(TChannel))
                ?? RegisterChannel<TChannel>(channelId);
            if (!timeOut.HasValue || timeOut.Value.TotalMilliseconds == 0)
            {
                timeOut = registeredChannel.Channel.GetChannelTimout();
                if (timeOut.Value.TotalMilliseconds == 0)
                {
                    timeOut = new TimeSpan(0, 0, 1);
                }
            }
            Request<TRequest, TResponse> requestMessage = new Request<TRequest, TResponse>
            {
                RequestContent = request,
            };
            var callWait = new RegisteredCallWait { Id = requestMessage.Id };
            var t = Task.Run(async () => await SafeCall<TChannel, TRequest, TResponse>(registeredChannel, callWait, requestMessage, timeOut));            
            t.Wait();
            var responseMessage = (Response<TResponse>)callWait.Response;
            if (!responseMessage.Ok)
            {
                throw responseMessage.Exception;
            }
            VrhLogger.Log("EventHub Call success.",
                new Dictionary<string, string>()
                {
                    {"Channel id", registeredChannel.Id},
                    {"Request", JsonConvert.SerializeObject(requestMessage) },
                    {"Response", JsonConvert.SerializeObject(responseMessage) },
                },
                null, LogLevel.Verbose, typeof(EventHubCore));
            return responseMessage.ResponseContent;
        }

        private static async Task SafeCall<TChannel, TRequest, TResponse>(ChannelRegister registeredChannel, RegisteredCallWait callWait, Request<TRequest, TResponse> requestMessage, TimeSpan? timeOut = null)
            where TRequest : new()
            where TResponse : new()
            where TChannel : BaseChannel, new()
        {
            lock (registeredChannel.Channel.CallWaits)
            {
                registeredChannel.Channel.CallWaits.Add(callWait);
            }
            try
            {
                try
                {                    
                    Send<TChannel, TRequest, TResponse>(registeredChannel.Id, requestMessage);
                    if (!(await callWait.WaitResponseSemaforSlim.WaitAsync(timeOut.Value)))
                    {
                        callWait.Response = new Response<TResponse>()
                        {
                            Exception = new FatalEventHubException("Call Request Timout occured!")
                        };
                    }
                }
                finally
                {
                    lock (registeredChannel.Channel.CallWaits)
                    {
                        registeredChannel.Channel.CallWaits.Remove(callWait);
                    }
                }
            }
            catch (Exception e)
            {
                VrhLogger.Log("EventHub Call Error!",
                    new Dictionary<string, string>()
                    {
                            {"Channel id", registeredChannel.Id},
                            {"Request", JsonConvert.SerializeObject(requestMessage) },
                    },
                    e,
                    LogLevel.Fatal,
                    typeof(EventHubCore));
                throw e;
            }
        }

        /// <summary>
        /// Regisztrál egy olyan üzenetketzelőt, 
        ///     amelynek nincs vissaztérési értéke és egy adott típusú beérkező üzenetet kezel
        /// Aszinkron, (feltehetően visszajelzés nélküli) üzenetfeldolgozás implementálásához
        /// <typeparam name="TChannel">Csatorna típusa, emely felett az üzenetkezelő működik</typeparam>
        /// <typeparam name="TMessage">Üzenet típusa, emylet ez a kezelő dolgoz fel</typeparam>
        /// <param name="channelId">Csatorna azonosító: ezen a csatornán érkező üzeneteket dolgozza fel a kezelő</param>
        /// <param name="handler">Metódus referencia, ez a metódus végzi a tényleges feldolgozást --> void X(TMessage)</param>
        /// </summary>
        public static void RegisterHandler<TChannel, TMessage>(string channelId, Action<TMessage> handler)
            where TMessage : new()
            where TChannel : BaseChannel, new()
        {
            RegisterHandler<TChannel>(channelId, HandlerRegister.RegistHandler(handler));
        }

        /// <summary>
        /// Registrál a megadott csatorna fölött egy Requesteket fogadó üzenetkezelőt, 
        /// melynek nincs visszatérési értéke 
        /// Aszinkron Request/Response implementációkhoz a kérések fogadó oldalán
        /// </summary>
        /// <typeparam name="TChannel">Csatorna típus implementáció</typeparam>
        /// <typeparam name="TRequest">Kérés típusa</typeparam>
        /// <typeparam name="TResponse">Válasz típusa</typeparam>
        /// <param name="channelId">Csatorna azonosító</param>
        /// <param name="handler">üzenetkezelő --> void X(TRequest)</param>
        public static void RegisterHandler<TChannel, TRequest, TResponse>(string channelId, Action<Request<TRequest, TResponse>> handler)
            where TRequest : new()
            where TResponse : new()
            where TChannel : BaseChannel, new()
        {
            RegisterHandler<TChannel>(channelId, HandlerRegister.RegistHandler(handler));
        }

        /// <summary>
        /// Registrál egy kifejezetten response üzeneteket kezelő handlert (nincs visszatérési értéke)
        /// Aszinkron Request/Response implementációkhoz
        /// </summary>
        /// <typeparam name="TChannel">Csatorna típus</typeparam>
        /// <typeparam name="TResponse">Válasz típusa</typeparam>
        /// <param name="channelId">Csatorna azonosító</param>
        /// <param name="handler">Üzenet kezelő --> void X(Response/TResponse/) </param>
        public static void RegisterHandler<TChannel, TResponse>(string channelId, Action<Response<TResponse>> handler)
            where TResponse : new()
            where TChannel : BaseChannel, new()
        {
            RegisterHandler<TChannel>(channelId, HandlerRegister.RegistHandler(handler));
        }

        /// <summary>
        /// Registrál a megadott csatorna fölött egy üzenetkezelőt, amelyik egy adott típusú kérést kezel, 
        ///     és egy adott típusú (ehhez a kéréshez tartozó) válaszszal tér vissza.
        /// Az ilyen üzenetkezelők a Call hívások fogadó oldalán kerülnek meghívásra!
        /// </summary>
        /// <typeparam name="TChannel">Csatorna típusa</typeparam>
        /// <typeparam name="TRequest">Kérés típusa</typeparam>
        /// <typeparam name="TResponse">Válasz típusa</typeparam>
        /// <param name="channelId">Csatorna azonosító</param>
        /// <param name="handler">Üzenet kezelő --> Tesponse/TResponse/ X(Request/TRequest, TResponse/) </param>
        public static void RegisterHandler<TChannel, TRequest, TResponse>(
            string channelId, Func<Request<TRequest, TResponse>, Response<TResponse>> handler)
            where TRequest : new()
            where TResponse : new()
            where TChannel : BaseChannel, new()
        {
            RegisterHandler<TChannel>(channelId, HandlerRegister.RegistHandler(handler));
        }

        /// <summary>
        /// Kiregisztrálja (eldobja) a megadott csatornán az üzenettípushoz regisztrált handlert
        /// </summary>
        /// <typeparam name="TChannel">Csatorna típusa</typeparam>
        /// <typeparam name="TMessage">Üzenet típusa</typeparam>
        /// <param name="channelId">csatorna azonosító (erről a csatornáról távolítja el a kezelőt)</param>
        /// <param name="handler">az eltávolítandő kezelő</param>
        public static void DropHandler<TChannel, TMessage>(string channelId, Action<TMessage> handler)
            where TMessage : new()
            where TChannel : BaseChannel, new()
        {
            DropHandler<TChannel>(channelId, HandlerRegister.RegistHandler(handler));
        }


        /// <summary>
        /// Kiregisztrálja (eldobja) a megadott csatornán az üzenettípushoz regisztrált handlert
        /// </summary>
        /// <typeparam name="TChannel">Csatorna típusa</typeparam>
        /// <typeparam name="TRequest">Kérés típusa</typeparam>
        /// <typeparam name="TResponse">Válasz típusa</typeparam>
        /// <param name="channelId">csatorna azonosító</param>
        /// <param name="handler">üzenet kezelő metodus, amit kiregisztrál</param>
        public static void DropHandler<TChannel, TRequest, TResponse>(string channelId, Action<Request<TRequest, TResponse>> handler)
            where TRequest : new()
            where TResponse : new()
            where TChannel : BaseChannel, new()
        {
            DropHandler<TChannel>(channelId, HandlerRegister.RegistHandler(handler));
        }

        /// <summary>
        /// Kiregisztrálja (eldobja) a megadott csatornán az üzenettípushoz regisztrált handlert
        /// </summary>
        /// <typeparam name="TChannel">Csatorna típusa</typeparam>
        /// <typeparam name="TResponse">Üzenet típusa</typeparam>
        /// <param name="channelId">csatorna azonosító</param>
        /// <param name="handler">üzenet kezelő metodus, amit kiregisztrál</param>
        public static void DropHandler<TChannel, TResponse>(string channelId, Action<Response<TResponse>> handler)
            where TResponse : new()
            where TChannel : BaseChannel, new()
        {
            DropHandler<TChannel>(channelId, HandlerRegister.RegistHandler(handler));
        }

        /// <summary>
        /// Kiregisztrálja (eldobja) a megadott csatornán az üzenettípushoz regisztrált handlert
        /// </summary>
        /// <typeparam name="TChannel">Csatorna típusa</typeparam>
        /// <typeparam name="TRequest">Kérés típusa</typeparam>
        /// <typeparam name="TResponse">Válasz típusa</typeparam>
        /// <param name="channelId">csatorna azonosító</param>
        /// <param name="handler">üzenet kezelő metodus, amit kiregisztrál</param>
        public static void DropHandler<TChannel, TRequest, TResponse>(
            string channelId, Func<Request<TRequest, TResponse>, Response<TResponse>> handler)
            where TRequest : new()
            where TResponse : new()
            where TChannel : BaseChannel, new()
        {
            DropHandler<TChannel>(channelId, HandlerRegister.RegistHandler(handler));
        }

        /// <summary>
        /// Az összes handlert kiregisztrálja + megszünteti a csatornát
        /// </summary>
        /// <param name="channelId">Csatorna, amit megszüntetünk.</param>
        public static void DropChannel<TChannel>(string channelId)
            where TChannel : BaseChannel, new()
        {
            var logData = new Dictionary<string, string>
            {
                { "Channel id", channelId },
                { "Channel type", typeof(TChannel).GetType().AssemblyQualifiedName },
            };
            var channel = _channels.FirstOrDefault(x => x.Id == channelId && x.Channel.GetType() == typeof(TChannel));
            if (channel != null)
            {
                lock (_channels)
                {
                    channel.Dispose();
                    _channels.Remove(channel);
                }
                VrhLogger.Log("Eventhub channel is unregistered with successful.", logData, null, LogLevel.Information, typeof(EventHubCore));
            }
            else
            {
                EventHubException.ThrowOrNo(
                    new EventHubException("Channel not found!"),
                    typeof(EventHubCore),
                    logData,
                    true);
            }
        }

        /// <summary>
        /// Eventhub üzenet elküldése adott csatorna implementáción
        /// </summary>
        /// <typeparam name="TChannel">Csatorna implementáció típusa</typeparam>
        /// <typeparam name="TMessage">Küldendő üzenet típus</typeparam>
        /// <param name="channelId">Csatorna azonosító, ahová küldi</param>
        /// <param name="message">Üzenet</param>
        private static void EventHubSend<TChannel, TMessage>(string channelId, object message)
            where TMessage : new()
            where TChannel : BaseChannel, new()
        {
            string serializedMessage = JsonConvert.SerializeObject(message);
            var logData =
                    new Dictionary<string, string>()
                    {
                        {"Channel id", channelId},
                        {"Message to send", serializedMessage }
                    };
            try
            {
                var registeredChannel =
                    _channels.FirstOrDefault(x => x.Id == channelId && x.Channel.GetType() == typeof(TChannel))
                    ?? RegisterChannel<TChannel>(channelId);
                string returnType = typeof(void).AssemblyQualifiedName;
                if (message.GetType().IsGenericType && message.GetType().GetGenericTypeDefinition() == typeof(Request<,>))
                {
                    var retType = message.GetType().GenericTypeArguments[1];
                    var responseGeneric = typeof(Response<>);
                    Type[] typeArgs = { retType };
                    Type responseType = responseGeneric.MakeGenericType(typeArgs);
                    returnType = responseType.AssemblyQualifiedName;
                }
                var eventHubMessage =
                    new EventHubMessage
                    {
                        ConcrateMessage = serializedMessage,
                        MessageType = message.GetType().AssemblyQualifiedName,
                        ReturnType = returnType,
                    };
                logData.Add("Message type", eventHubMessage.MessageType);
                logData.Add("Result type", eventHubMessage.ReturnType);
                // SEND this
                try
                {
                    registeredChannel.Channel.Send(eventHubMessage);
                }
                catch (Exception e)
                {
                    throw new FatalEventHubException($"Channel Send Error in {registeredChannel.Id} (type: {registeredChannel.Channel.GetType().FullName}) channel!", e);
                }
                VrhLogger.Log("Eventhub Send success.", logData, null, LogLevel.Information, typeof(EventHubCore));
            }
            catch (Exception e)
            {
                EventHubException.ThrowOrNo(
                    new FatalEventHubException("Eventhub Send exception occured!", e), typeof(EventHubCore), logData);
            }
        }

        /// <summary>
        /// Regisztrálja a csatornát
        /// </summary>
        /// <typeparam name="TChannel">Létező csatorna implementáció</typeparam>
        /// <param name="channelId">Csatorna azonosító</param>
        /// <returns>Regisztrált csatorna</returns>
        private static ChannelRegister RegisterChannel<TChannel>(string channelId)
            where TChannel : BaseChannel, new()
        {
            var channel = ChannelRegister.CreateChannel<TChannel>(channelId);
            lock (_channels)
            {
                _channels.Add(channel);
            }
            VrhLogger.Log("Register EventHub channel is success!",
                new Dictionary<string, string>
                {
                    { "Channel id", channelId },
                    { "Channel type", typeof(TChannel).AssemblyQualifiedName }
                },
                null, LogLevel.Information, typeof(EventHubCore));
            return channel;
        }

        /// <summary>
        /// Registrál egy message handlert az eventhubon a megadott csatornához
        /// </summary>
        /// <typeparam name="TChannel">A csatorna típusa, main a handler működik</typeparam>
        /// <param name="channelId">csatorna azonosítója</param>
        /// <param name="handlerRegister">üzenetkezelő regisztrációs objektum</param>
        private static void RegisterHandler<TChannel>(string channelId, HandlerRegister handlerRegister)
            where TChannel : BaseChannel, new()
        {
            var registeredChannel =
                _channels.FirstOrDefault(x => x.Id == channelId && x.Channel.GetType() == typeof(TChannel))
                ?? RegisterChannel<TChannel>(channelId);
            HandlerRegister existingHandlerRegister = null;
            lock (registeredChannel.Channel.Handlers)
            {
                existingHandlerRegister =
                    registeredChannel.Channel.Handlers.FirstOrDefault(x => x.MessageType == handlerRegister.MessageType
                                                                            && x.ReturnType == handlerRegister.ReturnType);
                if (existingHandlerRegister != null)
                {
                    existingHandlerRegister.Dispose();
                    registeredChannel.Channel.Handlers.Remove(existingHandlerRegister);
                }
                registeredChannel.Channel.Handlers.Add(handlerRegister);
            }
            var logData = new Dictionary<string, string>
            {
                { "Channel id", channelId },
                { "Channel type", typeof(TChannel).AssemblyQualifiedName },
                { "Handler methode", handlerRegister.Handler.Method.Name },
                { "Class of handler", handlerRegister.Handler.Method.DeclaringType.AssemblyQualifiedName },
                { "Handled message type", handlerRegister.MessageType.AssemblyQualifiedName },
                { "Result type", handlerRegister.ReturnType.AssemblyQualifiedName },
            };
            if (existingHandlerRegister != null)
            {
                VrhLogger.Log("Handler already registered, and early registration is overwritten.",
                    logData, null, LogLevel.Warning, typeof(EventHubCore));
            }
            VrhLogger.Log("Handler is registered with successful.",
                logData, null, LogLevel.Information, typeof(EventHubCore));
        }

        /// <summary>
        /// Kiregisztrál egy handlert az eventhubon a megadott csatornához
        /// </summary>
        /// <typeparam name="TChannel">Csatorna típusa</typeparam>
        /// <param name="channelId">Csatorna azonosítója</param>
        /// <param name="handlerRegister">kiregisztráélandó handler</param>
        private static void DropHandler<TChannel>(string channelId, HandlerRegister handlerRegister)
            where TChannel : BaseChannel, new()
        {
            var logData = new Dictionary<string, string>
            {
                { "Channel id", channelId },
                { "Cahnnel type", typeof(TChannel).AssemblyQualifiedName },
                { "Handler methode", handlerRegister.Handler.Method.Name },
                { "Clas of handler", handlerRegister.Handler.Method.DeclaringType.AssemblyQualifiedName },
                { "Handled message type", handlerRegister.MessageType.AssemblyQualifiedName },
                { "Result type", handlerRegister.ReturnType.AssemblyQualifiedName },
            };
            var registeredChannel =
                _channels.FirstOrDefault(x => x.Id == channelId && x.Channel.GetType() == typeof(TChannel));
            if (registeredChannel != null)
            {
                HandlerRegister existingHandlerRegister = null;
                lock (registeredChannel.Channel.Handlers)
                {
                    existingHandlerRegister =
                        registeredChannel.Channel.Handlers.FirstOrDefault(x => x.MessageType == handlerRegister.MessageType
                                                                                && x.ReturnType == handlerRegister.ReturnType);
                    if (existingHandlerRegister != null)
                    {
                        existingHandlerRegister.Dispose();
                        registeredChannel.Channel.Handlers.Remove(existingHandlerRegister);
                    }
                }
                if (existingHandlerRegister != null)
                {
                    VrhLogger.Log("Handler is dropped with successful.",
                        logData, null, LogLevel.Information, typeof(EventHubCore));
                }
                else
                {
                    VrhLogger.Log("Handler is not dropped, because this handler is not exist.",
                        logData, null, LogLevel.Warning, typeof(EventHubCore));
                }
            }
            else
            {
                VrhLogger.Log("Handler is not dropped, because channel is not exist.",
                    logData, null, LogLevel.Warning, typeof(EventHubCore));
            }
        }

        /// <summary>
        /// Csatornák listája
        /// </summary>
        private static readonly List<ChannelRegister> _channels = new List<ChannelRegister>();

        /// <summary>
        /// EventHub core beállításai
        /// </summary>
        private static readonly Lazy<EventHubCoreConfig> _lazyEventHubCoreConfiguration = new Lazy<EventHubCoreConfig>(() =>
        {
            string config = ConfigurationManager.AppSettings[$"{MODUL_PREFIX}:Config"];
            if (String.IsNullOrEmpty(config))
            {
                config = "Vrh.EventHub.Core.Config.xml/Vrh.EventHub.Core";
            }
            VrhLogger.Log($"Used EventHub configuration: {config}", LogLevel.Information, typeof(EventHubCore));
            return new EventHubCoreConfig(config);
        });

        /// <summary>
        /// Az eventhub konfigurációja
        /// </summary>
        internal static EventHubCoreConfig EventHubCoreConfiguration => _lazyEventHubCoreConfiguration.Value;

        /// <summary>
        /// Modul prefix
        /// </summary>
        internal const string MODUL_PREFIX = "Vrh.EventHub.Core";
    }
}
