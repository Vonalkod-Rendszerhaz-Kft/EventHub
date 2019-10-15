using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vrh.EventHub.Protocols.RedisPubSub;
using Vrh.EventHub.Core;

namespace Vrh.EventHub.Intervention
{
    /// <summary>
    /// Egyszerűsített "beavatkozás" EventHub implemntácoió
    ///     Korlátozások:
    ///         - Csak Redis Pub/Sub channel-lel működik
    ///         - Csak szinkron (Call) hívásokat támogat
    ///         - Nem támogat fogadó oldalon ugyanarra a típusra többszörös Handler regisztrációt ugyanarra a csatornára, ugyanabban az alkalmazástérben
    /// </summary>
    public static class EventHubIntervention
    {
        /// <summary>
        /// Registrál egy kezelő metódust az adott nevű csatornára érkező összes Beavatkozás számára
        ///     (Feldolgozó oldalon használjuk!)
        /// </summary>
        /// <param name="channelId">Csatorna azonosító</param>
        /// <param name="handler">A regisztrált handler (üzenetfogadó/feldolgozó metódus</param>
        public static void RegisterInterventionChannel(
            string channelId, Func<string, Dictionary<string, string>, Dictionary<string, string>> handler)
        {
            lock (staticLocker)
            {
                var existingHandler = ivHandlerRegistrationStore.FirstOrDefault(x => x.ChannelId == channelId);
                if (existingHandler != null)
                {
                    DropInterventionChannelRegistration(channelId);
                }
                EventHubCore.RegisterHandler<RedisPubSubChannel,
                                                InterventionContract.InterventionRequest,
                                                InterventionContract.InterventionResponse>(channelId, HandlerDistributorFunc);
                ivHandlerRegistrationStore.Add(new InterventionHandlerRegistration() { ChannelId = channelId, Handler = handler });
            }
        }

        /// <summary>
        /// Eldobja az adott nevű beavatkozás csatornát
        ///     (Feldolgozó oldalon használjuk!)
        /// </summary>
        /// <param name="channelId">A megszüntetendő beavatkozás csatorna</param>
        public static void DropInterventionChannelRegistration(string channelId)
        {
            lock(staticLocker)
            {
                var existingHandler = ivHandlerRegistrationStore.FirstOrDefault(x => x.ChannelId == channelId);
                if (existingHandler != null)
                {
                    EventHubCore.DropHandler<RedisPubSubChannel, 
                                                InterventionContract.InterventionRequest, 
                                                InterventionContract.InterventionResponse>(channelId, HandlerDistributorFunc);
                    ivHandlerRegistrationStore.Remove(existingHandler);
                }                
            }
        }

        /// <summary>
        /// Beavatkozás meghívása
        ///     (Hívó oldalon használjuk!)
        /// </summary>
        /// <param name="channelId">A csatorna amire behívunk</param>
        /// <param name="interventionName">A bevatkozás neve (azonosítója)</param>
        /// <param name="parameters">A beavatkozás paraméterei</param>
        /// <param name="timeout">Ebben a hívásban használni kívánt timeout (ha nincs megadva, akkor a csatorna szerinti default-ot használja)</param>
        /// <returns>A beavatkozás által visszaadott adatok</returns>
        public static Dictionary<string, string> RunIntervention(
            string channelId, string interventionName, Dictionary<string, string> parameters, TimeSpan? timeout = null)
        {
            var request = new InterventionContract.InterventionRequest()
            {
                InterventionName = interventionName,
                Parameters = parameters,                
                ChannelId = channelId,
            };
            var response = EventHubCore.Call<RedisPubSubChannel, InterventionContract.InterventionRequest, InterventionContract.InterventionResponse>(channelId, request, timeout);
            return response.Data;
        }


        private static Response<InterventionContract.InterventionResponse> HandlerDistributorFunc(
            Request<InterventionContract.InterventionRequest, InterventionContract.InterventionResponse> request)
        {
            lock (staticLocker)
            {
                var response = request.MyResponse;
                var handler = ivHandlerRegistrationStore.FirstOrDefault(x => x.ChannelId == request.RequestContent.ChannelId);
                if (handler != null)
                {
                    try
                    {
                        var responseData = handler.Handler.Invoke(request.RequestContent.InterventionName, request.RequestContent.Parameters);
                        response.ResponseContent = new InterventionContract.InterventionResponse
                        {
                            Data = responseData
                        };
                    }
                    catch(Exception ex)
                    {
                        response.Exception = ex;
                    }
                }
                else
                {
                    response.Exception = new Exception($"Internal Error! Intervention handler is not registered for {request.RequestContent.ChannelId} intervention channel!");
                }
                return response;
            }
        }

        /// <summary>
        /// Végpontban (alkalmazástérben) létező beavatkozás csatornák listája
        /// </summary>
        private static readonly List<InterventionHandlerRegistration> ivHandlerRegistrationStore = new List<InterventionHandlerRegistration>();

        /// <summary>
        /// Osztály szintű locker
        /// </summary>
        private static readonly object staticLocker = new object(); 
    }
}
