using System;

namespace Vrh.EventHub.Core
{
    /// <summary>
    /// Handler nyilvántartására szolgáló osztály
    /// </summary>
    internal class HandlerRegister : IDisposable
    {
        /// <summary>
        /// Gyártó függvény, olyan handler regisztrációjához, 
        ///     amelynek nincs vissaztérési értéke és egy adott típusú beérkező üzenetet kezel
        /// Aszinkron, (feltehetően visszajelzés nélküli) üzenetfeldolgozás implementálásához
        /// </summary>
        /// <typeparam name="TMessage">Üzenet típus, melre a kezelő kötve van</typeparam>
        /// <param name="handler">Üzenet kezelő metódus referencia</param>
        /// <returns>Handler nyilvántartó példány</returns>
        internal static HandlerRegister RegistHandler<TMessage>(Action<TMessage> handler)
            where TMessage : new()
        {
            return new HandlerRegister(handler, typeof(TMessage));
        }

        /// <summary>
        /// Gyártó függvény, olyan handler regisztrációjához, 
        ///     amelynek nincs vissaztérési értéke és egy beérkező Request kezel
        /// Aszinkron Request/Response logika implementálásához
        /// </summary>
        /// <typeparam name="TRequest">A kérés típusa</typeparam>
        /// <typeparam name="TResponse">A kéréshez tartozó válasz típusa</typeparam>
        /// <param name="handler">Metódus refernecia amely kezeli az érkezó üzenetet</param>
        /// <returns>Handler nyilvántartó példány</returns>
        internal static HandlerRegister RegistHandler<TRequest, TResponse>(Action<Request<TRequest, TResponse>> handler)
            where TRequest : new()
            where TResponse : new()
        {                             
            return new HandlerRegister(handler, typeof(Request<TRequest, TResponse>));
        }

        /// <summary>
        /// Gyártő függvény response kezeléshez 
        ///     (ilyen handler regisztrációk csak aszinkron request-response megavlósításoknál léteznek
        ///     ezeknek a handlereknek sosincs visszatérési értékük)
        /// </summary>
        /// <typeparam name="TResponse">Kérésre adott válasz</typeparam>
        /// <param name="handler">Az üzenet kezelő handler (metódus)</param>
        /// <returns>Legyártott handler regisuzter példány, ami nyilvántartja ezt az üzenetkeezlő regisztrációt</returns>
        internal static HandlerRegister RegistHandler<TResponse>(Action<Response<TResponse>> handler)
            where TResponse : new()
        {
            return new HandlerRegister(handler, typeof(Response<TResponse>));
        }

        /// <summary>
        /// Gyártó függvény, olyan handler regisztrációjához, amelyiknek van vissaztérési értéke
        /// Szinkron hívások (Call) kezelésre
        /// </summary>
        /// <typeparam name="TRequest">Típus, amelyhez a handler tartozik</typeparam>
        /// <typeparam name="TResponse">Típus, amelyel a handler visszatér</typeparam>
        /// <param name="handler">Metódus refernecia amely kezeli az érkezó üzenetet</param>
        /// <returns>Handler nyilvántartó példány</returns>
        internal static HandlerRegister RegistHandler<TRequest, TResponse>(Func<Request<TRequest, TResponse>, Response<TResponse>> handler)
            where TRequest : new()
            where TResponse : new()
        {
            return new HandlerRegister(handler, typeof(Request<TRequest, TResponse>));
        }

        /// <summary>
        /// private constructor
        /// Ezt a típust csak a gyártófügvényén át lehet kivülről példányosítani!!!
        /// </summary>
        /// <param name="handler">Metódus refernecia amely kezeli az érkezó üzenetet</param>
        /// <param name="handedMessageType">Kezelt üzenet típus</param>
        private HandlerRegister(Delegate handler, Type handedMessageType)
        {
            Handler = handler;
            MessageType = handedMessageType;
            ReturnType = handler.Method.ReturnType;
        }

        /// <summary>
        /// Üzenet típus, melyet kezel a handler
        /// </summary>
        public Type MessageType { get; }

        /// <summary>
        /// A handler visszatérési típusa
        /// </summary>
        public Type ReturnType { get; }

        /// <summary>
        /// Metódus refernecia amely kezeli az érkezó üzenetet
        /// </summary>
        public Delegate Handler { get; private set; }

        #region IDisposable Support
        private bool _disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    Handler = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~HandlerRegister() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
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

