using System;

namespace Vrh.EventHub.Core
{
    /// <summary>
    /// Nyilvántart egy eventhub csatornát
    /// </summary>
    internal class ChannelRegister : IDisposable
    {
        /// <summary>
        /// Static channel factory (generic)
        /// </summary>
        /// <typeparam name="TChannel">A csatorna típusa</typeparam>
        /// <param name="id">csatorna azonosítója</param>
        /// <returns></returns>
        internal static ChannelRegister CreateChannel<TChannel>(string id)
            where TChannel : BaseChannel, new()
        {
            var ch = new TChannel();
            ch.ChannelId = id;
            ch.InitializeChannelInfrastructure();         
            return new ChannelRegister(id, ch);
        }

        /// <summary>
        /// ChannelRegister
        /// </summary>
        /// <param name="id">csatorna azonosítója</param>
        /// <param name="channel">Csatorna példány</param>
        private ChannelRegister(string id, BaseChannel channel)
        {
            Id = id;
            Channel = channel;
        }

        /// <summary>
        /// Csatorna azonosító
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Csatorna példány
        /// </summary>
        public BaseChannel Channel { get; }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    foreach (var handler in Channel.Handlers)
                    {
                        handler.Dispose();
                    }
                    Channel.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ChannelRegister() {
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
