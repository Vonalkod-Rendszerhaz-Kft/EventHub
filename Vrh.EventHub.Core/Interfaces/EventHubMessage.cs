namespace Vrh.EventHub.Core
{
    /// <summary>
    /// Eventhub channel objektum, amelyben az üzenetek közlekednek az evenethub-on
    /// </summary>
    public class EventHubMessage
    {
        /// <summary>
        /// Üzenet típusa
        /// </summary>
        public string MessageType { set; get; }

        /// <summary>
        /// Az üzenet feldolgozás visszatérési típusa
        /// </summary>
        public string ReturnType { get; set; }

        /// <summary>
        /// Maga az üzenet (JSON-szerializált string)
        /// </summary>
        public string ConcrateMessage { set; get; }
    }
}
