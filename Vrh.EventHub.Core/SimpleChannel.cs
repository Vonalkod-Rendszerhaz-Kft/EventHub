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

        /// <summary>
        /// SimpleChannel osztály - egyszerűsített üzenetkezelés
        /// </summary>
        public static class SimpleChannel
        {
            public class UndefinedInterventionException : Exception
            {
                public UndefinedInterventionException(string ivname) : base($"UndefinedInterventionException: {ivname}") { }
            }
            private class InputParameters
            {
                public InputParameters() { }
                public string ivName = null;
                public Dictionary<string, string> Parameters = new Dictionary<string, string>() { };
            }
            public static Dictionary<string, string> Call(string channelId, string ivname, Dictionary<string, string> par, TimeSpan? ts = null)
            {
                var sfip = new InputParameters();
                sfip.ivName = ivname;
                var rq = new Request<InputParameters, Dictionary<string, string>> { };
                var rp = EventHubCore.Call<RedisPubSubChannel,
                    Request<InputParameters, Dictionary<string, string>>,
                    Response<Dictionary<string, string>>>(channelId, rq, ts);
                return rp.ResponseContent;
            }

            public static Func<string, Dictionary<string, string>, Dictionary<string, string>> userfunchandler;
            public static void RegisterHandler(
                    string channelId,
                    Func<string, Dictionary<string, string>, Dictionary<string, string>> handler)
            {
                userfunchandler = handler;
                EventHubCore.RegisterHandler<RedisPubSubChannel,
                    InputParameters,
                    Dictionary<string, string>>(channelId, SimpleFunc);
            }
            private static Response<Dictionary<string, string>> SimpleFunc(Request<InputParameters, Dictionary<string, string>> request)
            {
                var myResponse = request.MyResponse;
                try
                {
                    myResponse.ResponseContent = userfunchandler(request.RequestContent.ivName, request.RequestContent.Parameters);
                }
                catch (Exception ex)
                {
                    myResponse.Exception = ex;
                }
                return myResponse;
            }
        }
    }
}
