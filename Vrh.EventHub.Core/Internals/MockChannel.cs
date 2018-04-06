using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Vrh.EventHub.Core
{
    /// <summary>
    /// Unit teszthez használt mock csatorna implementáció, csak alaklamazáson (alkalmazástéren) belül működik!!!
    /// </summary>
    internal class MockChannel : BaseChannel
    {
        /// <summary>
        /// Send (Aszinkron) üzenetküldés csatornaimplementáció
        /// </summary>
        /// <param name="message">üzenet</param>
        public override void Send(EventHubMessage message)
        {
            Task.Run(() =>
                            HandleInputMessages(
                                JsonConvert.SerializeObject(message))
                     );
        }

        /// <summary>
        /// Inicializálja a csatorna infrastruktúráját
        /// </summary>
        public override void InitializeChannelInfrastructure()
        {
            base.InitializeChannelInfrastructure();
        }

        /// <summary>
        /// Üzenet érkezik  acsatornára
        /// </summary>
        /// <param name="message">Érkezett üzenet (json serializált )</param>
        private void HandleInputMessages(string message)
        {
            // csak átpaszolja (aszinkron) az érkező üzenetet a Core-nak
            EventHubMessage msg = JsonConvert.DeserializeObject<EventHubMessage>(message);
            Task.Run(() => _coreInputMessageHandler.Invoke(msg));
        }

        #region IDisposable Support
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        #endregion IDisposable Support
    }
}
