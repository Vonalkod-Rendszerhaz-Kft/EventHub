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
using Vrh.EventHub.Protocols.RedisPubSub;
using Vrh.EventHub.Core;

namespace Vrh.EventHub.RedisPubSub.Simple
{
    /// <summary>
    /// SimpleChannel osztály - egyszerűsített üzenetkezelés
    /// </summary>
    public class SimpleChannel
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
        public Dictionary<string, string> Call(string channelId, string ivname, Dictionary<string, string> par, TimeSpan? ts = null)
        {
            var sfip = new InputParameters();
            sfip.ivName = ivname;
            var rq = new Request<InputParameters, Dictionary<string, string>> { };
            var rp = EventHubCore.Call<RedisPubSubChannel,
                Request<InputParameters, Dictionary<string, string>>,
                Response<Dictionary<string, string>>>(channelId, rq, ts);
            return rp.ResponseContent;
        }

        public Dictionary<string,Func<string, Dictionary<string, string>, Dictionary<string, string>>> userfunchandlerList = new Dictionary<string,Func<string, Dictionary<string, string>, Dictionary<string, string>>>();
        public void RegisterHandler(
                string channelId,
                Func<string, Dictionary<string, string>, Dictionary<string, string>> handler)
        {
            if (!userfunchandlerList.ContainsKey(channelId)) userfunchandlerList.Add(channelId,handler);
            EventHubCore.RegisterHandler<RedisPubSubChannel,
                InputParameters,
                Dictionary<string, string>>(channelId, SimpleFunc);
        }
        private Response<Dictionary<string, string>> SimpleFunc(Request<InputParameters, Dictionary<string, string>> request)
        {
            var myResponse = request.MyResponse;
            try
            {
                myResponse.ResponseContent = userfunchandlerList["ABC"](request.RequestContent.ivName, request.RequestContent.Parameters);
            }
            catch (Exception ex)
            {
                myResponse.Exception = ex;
            }
            return myResponse;
        }
    }
}
