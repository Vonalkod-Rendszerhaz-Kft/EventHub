using Newtonsoft.Json;
using System;

namespace Vrh.EventHub.Core
{
    /// <summary>
    /// Egy olyan üzenet, amelre a feldolgozó oldalnak választ kell adnia
    /// </summary>
    /// <typeparam name="TRequest">Kérés típusa</typeparam>
    /// <typeparam name="TResponse">Válasz típusa</typeparam>
    public class Request<TRequest, TResponse>
        where TRequest : new()
        where TResponse : new()
    {
        /// <summary>
        /// Üzenet azonosító
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Maga az üzenet
        /// </summary>
        public TRequest RequestContent { get; set; }

        /// <summary>
        /// Visszaad egy üres (kitöltetlen) response üzenetet. Amely ehhez az üzenethez (Request) tartozik
        ///     (A tipushellyeség elősegítésére és az üzenetpárosításra használandó fel!)
        /// </summary>
        [JsonIgnore]
        public Response<TResponse> MyResponse => Response<TResponse>.ResponseFactory(Id);
    }
}
