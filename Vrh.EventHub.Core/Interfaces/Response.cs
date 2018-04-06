using System;

namespace Vrh.EventHub.Core
{
    /// <summary>
    /// Válasz üzenet egy Eventhub üzenetre
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Response<T>
        where T : new()
    {
        /// <summary>
        /// Internal gyártó függvény
        ///     A Request típus használja, hogy a GetMyResponse visszaadja a kéréshez tartozó választ 
        /// </summary>
        /// <param name="requestId">kérés azonosító</param>
        /// <returns></returns>
        internal static Response<T> ResponseFactory(string requestId) => new Response<T>
        {
            RequestId = requestId
        };

        /// <summary>
        /// Maga az üzenet
        /// </summary>
        public T ResponseContent { get; set; }

        /// <summary>
        /// Sikeres a válasz, vagy hiba lépett fel? 
        /// </summary>
        public bool Ok => Exception == null;

        /// <summary>
        /// Hiba esetén kell tartalmazni egy Exceprtion-t, vagy egy Exception lezséármazottat
        /// Ha nincs hiba akkor kötelezően null
        /// </summary>
        public Exception Exception { get; set; } = null;

        /// <summary>
        /// Üzenet azonosító (a request-tel párosítja)
        /// </summary>
        public string RequestId { get; set; }
    }
}
