using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vrh.Logger;

namespace Vrh.EventHub.Core
{
    /// <summary>
    /// Alap csatorna implementáció
    /// Az EventHub megoldásban használható konkrét csatorna implementzációknak ebből kell származniuk!
    /// </summary>
    public class BaseChannel : IDisposable
    {
        /// <summary>
        /// Üzenetküldés a csatornán
        ///     Konkrét csatorna implementációban mindig felülírandó,
        ///     az adott csatornának megfelelő konkrét üzenetküldés implementációjával!!! 
        /// </summary>
        /// <param name="message">üzenet</param>
        public virtual void Send(EventHubMessage message)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Inicializálja a channel infrastruktúráját, 
        ///     a konkrét implementácoiója függ a konkrét csatorna megvalósítástól
        ///     A függvény hívását követően azonban az üzenettovábító infrastruktúrának maradéktalanul müködőképesnek kell lennie (startup)
        /// A csatorna megvalósításban a Core visszahhívási delegate mentéséhez mindig meg kellhívni a base-ben lévő implementácót!!! (base.InitializeChannelInfrastructure(eventHubCoreInputMessageHandler))
        /// </summary>
        public virtual void InitializeChannelInfrastructure()
        {
            _coreInputMessageHandler = HandleInputMessage;
            VrhLogger.Log("Base InitializeChannelInfrastructure called", LogLevel.Debug, this.GetType());
        }

        /// <summary>
        /// A beérkező üzenetek kezelője
        /// Ez a metódus gondoskodik róla, hogy meghívja az érketzett üzenethez tartozó Handlert
        /// Ha nincs várt visszatérési érték (a hívandó handler result-ja void), akkor aszinkronhívja
        /// Ha van várt visszatérési érték (a hívandó handler result-ja nem void), akkor szinkron hívhja, 
        ///     és a kapott választ elküldi a csatornára
        /// </summary>
        /// <param name="msg">EventHub üzenet</param>
        public void HandleInputMessage(EventHubMessage msg)
        {
            var logData = new Dictionary<string, string>
            {
                {"Message type", msg.MessageType },
                {"Result type", msg.ReturnType },
                {"Message content", msg.ConcrateMessage },
            };
            VrhLogger.Log("Message receive", logData, null, LogLevel.Debug, this.GetType());
            Type concrateMessageType = Type.GetType(msg.MessageType);
            var concrateMessage = JsonConvert.DeserializeObject(msg.ConcrateMessage, concrateMessageType);
            if (concrateMessageType.IsGenericType && concrateMessageType.GetGenericTypeDefinition() == typeof(Response<>))
            {
                string requestId = (string)concrateMessage.GetType()
                                        .GetProperty("RequestId")
                                        .GetValue(concrateMessage, null);
                RegisteredCallWait callWait = null; 
                lock (CallWaits)
                {
                    callWait = CallWaits.FirstOrDefault(x => x.Id == requestId);
                }
                if (callWait != null)
                {
                    callWait.Response = concrateMessage;
                    callWait.WaitResponseSemaforSlim.Release();
                    logData.Add("Request id", requestId);
                    VrhLogger.Log("Registered callwait found and signaled successfull.",
                        logData, null, LogLevel.Verbose, this.GetType());
                    return;
                }
            }
            IEnumerable<HandlerRegister> handlers = null;
            lock (Handlers)
            {
                handlers = Handlers.Where(x => x.MessageType.AssemblyQualifiedName == msg.MessageType
                                                        && x.ReturnType.AssemblyQualifiedName == msg.ReturnType);
            }
            if (handlers != null)
            {
                int i = 0;
                foreach (var handler in handlers)
                {
                    i++;
                    logData.Add($"Called handler #{i}", handler.Handler.Method.Name);
                    if (handler.ReturnType == typeof(void))
                    {
                        handler.Handler.Method.Invoke(handler.Handler.Target, (new object[] { concrateMessage }));
                        VrhLogger.Log("Message receive and call handler async succesfull.",
                            logData, null, LogLevel.Verbose, this.GetType());
                    }
                    else
                    {
                        try
                        {
                            var result = handler.Handler.Method.Invoke(handler.Handler.Target, new object[] { concrateMessage });
                            Send(new EventHubMessage()
                            {
                                ConcrateMessage = JsonConvert.SerializeObject(result),
                                MessageType = result.GetType().AssemblyQualifiedName,
                                ReturnType = typeof(void).AssemblyQualifiedName,
                            });
                        }
                        catch (Exception e)
                        {
                            VrhLogger.Log("Call handler succes, by unwanted exception recieve from handler methode!",
                                logData, e, LogLevel.Error, this.GetType());
                        }
                    }
                }
            }
            else
            {
                VrhLogger.Log("Message receive but handler not registered in this side of channel!",
                    logData, null, LogLevel.Debug, this.GetType());
            }
        }

        /// <summary>
        /// Csatorna timoutja (konkrét csatorna implementáción ovverride-olható)
        /// </summary>
        /// <returns>timeout</returns>
        public virtual TimeSpan GetChannelTimout()
        {
            return new TimeSpan(0, 0, 1);
        }

        /// <summary>
        /// Csatorna azonosító
        /// </summary>
        public string ChannelId { get; set; }

        /// <summary>
        /// Registrált üzenetkezelők listája
        ///  Üzenettovábbításhoz innen kell előszedni a handler delegate referenciát, amit hívni kell.
        ///  A deleget referencia kikeresésének idejére lockkolni kell! (lock(_handlers) { ... }) 
        /// Alapvetően az EventHub core keret és a BaseChannel kezeli!!!!
        /// </summary>
        internal List<HandlerRegister> Handlers { get; } = new List<HandlerRegister>();

        /// <summary>
        /// Regisztrált visszahívási szemaforok listája
        /// </summary>
        internal List<RegisteredCallWait> CallWaits { get; } = new List<RegisteredCallWait>();

        /// <summary>
        /// A bejövő üzeneteket kezelő metodus referenciája
        /// </summary>
        protected Action<EventHubMessage> _coreInputMessageHandler;

        #region IDisposable Support
        /// <summary>
        /// To detect redundant calls
        /// </summary>
        protected bool disposedValue = false;

        /// <summary>
        /// Leszármazottban felülírandó, és el kell végezni a csatorna specifikus clean-t, 
        ///     de a base.Dispose()-t mindig meg kell hívni a végén!!!
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    lock (Handlers)
                    {
                        // Üzenetfelkdolgozó kiregisztrálása
                        _coreInputMessageHandler = null;
                        // Összes handler eldobása
                        foreach (var handler in Handlers)
                        {
                            handler.Dispose();
                        }
                        Handlers.Clear();
                    }
                    VrhLogger.Log("Dispose Channel succesfull.",
                        new Dictionary<string, string>
                        {
                            { "Channel id", ChannelId },
                        },
                        null,
                        LogLevel.Verbose,
                        this.GetType()
                    );                    
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~BaseChannel() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        /// <summary>
        /// This code added to correctly implement the disposable pattern.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
