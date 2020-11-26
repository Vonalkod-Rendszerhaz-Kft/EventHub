using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vrh.EventHub.Core;
using System.Collections.Generic;

namespace Vrh.EventHub.Intervention.Test
{
    [TestClass]
    public class InterventionTests
    {
        public InterventionTests()
        {
            EventHubIntervention.RegisterInterventionChannel(CHANNEL_NAME, InterventoionHandlerForTest);
        }

        [TestMethod]
        public void RegisterInterventionHandlerAndWork()
        {
            Dictionary<string, string> result = CallIntervention(CHANNEL_NAME, 5);
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(Dictionary<string, string>));
            Assert.AreEqual(result[INTERVENTION_NAME_KEY], INTERVENTION_NAME);
            Assert.AreEqual(result[PARAMETER_NAME], PARAMETER_VALUE);
        }

        private static Dictionary<string, string> CallIntervention(string channelName, int timeOutInSec)
        {
            var parameters = new Dictionary<string, string>()
            {
                { PARAMETER_NAME, PARAMETER_VALUE }
            };
            var result = EventHubIntervention.RunIntervention(channelName, INTERVENTION_NAME, parameters, new TimeSpan(0, 0, timeOutInSec));
            return result;
        }

        [TestMethod]
        [ExpectedException(typeof(FatalEventHubException))]
        public void DropHandlerWork()
        {
            //EventHubIntervention.DropInterventionChannel(CHANNEL_NAME);
            CallIntervention(CHANNEL_NAME, 1);
        }

        [TestMethod]
        public void MultipleCahnnelWork()
        {
            EventHubIntervention.RegisterInterventionChannel(ANOTHER_CHANNEL_NAME, InterventoionHandlerForTest);
            Dictionary<string, string> result = CallIntervention(ANOTHER_CHANNEL_NAME, 5);
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(Dictionary<string, string>));
            Assert.AreEqual(result[INTERVENTION_NAME_KEY], INTERVENTION_NAME);
            Assert.AreEqual(result[PARAMETER_NAME], PARAMETER_VALUE);
        }

        [TestMethod]
        public void OverRegistratringWork()
        {
            EventHubIntervention.RegisterInterventionChannel(CHANNEL_NAME, OtherInterventoionHandlerForTest);
            Dictionary<string, string> result = CallIntervention(CHANNEL_NAME, 5);
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(Dictionary<string, string>));
            Assert.AreEqual(result[INTERVENTION_NAME_KEY], INTERVENTION_NAME);
            Assert.AreEqual(result[INTERVENTION_HANDLER_NAME_KEY], nameof(InterventionTests.OtherInterventoionHandlerForTest));
            Assert.AreEqual(result[PARAMETER_NAME], PARAMETER_VALUE);
        }

        [TestMethod]
        public void ExceptionsWork()
        {
            EventHubIntervention.RegisterInterventionChannel(AGAIN_ANOTHER_CHANNEL_NAME, InterventoionHandler);
            try
            {
                EventHubIntervention.RunIntervention(AGAIN_ANOTHER_CHANNEL_NAME, "unknow_iw", new Dictionary<string, string>(), new TimeSpan(0, 0, 10));
            }
            catch(Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(Exception));
                Assert.AreEqual("1", ex.Message);
            }
        }

        private Dictionary<string, string> InterventoionHandler(string interventionName, Dictionary<string, string> parameters)
        {
            var returnData = new Dictionary<string, string>();
            switch (interventionName)
            {
                case "iv1":
                    // process: do something!
                    returnData.Add("intervention", "iv1");
                    break;
                case "iv2":
                    // process: do something!
                    returnData.Add("intervention", "iv2");
                    break;
                default:
                    throw new Exception("1");
            }
            return returnData;
        }

        private Dictionary<string, string> InterventoionHandlerForTest(string interventionName, Dictionary<string, string> parameters)
        {
            var result = new Dictionary<string, string>
            {
                { INTERVENTION_NAME_KEY, interventionName }
            };
            foreach (var item in parameters)
            {
                result.Add(item.Key, item.Value);
            }
            return result;
        }

        private Dictionary<string, string> OtherInterventoionHandlerForTest(string interventionName, Dictionary<string, string> parameters)
        {
            var result = new Dictionary<string, string>
            {
                { INTERVENTION_NAME_KEY, interventionName },
                { INTERVENTION_HANDLER_NAME_KEY, nameof(InterventionTests.OtherInterventoionHandlerForTest) }
            };
            foreach (var item in parameters)
            {
                result.Add(item.Key, item.Value);
            }
            return result;
        }

        private const string CHANNEL_NAME = "any channel name";
        private const string ANOTHER_CHANNEL_NAME = "another channel name";
        private const string AGAIN_ANOTHER_CHANNEL_NAME = "again other channel name";

        private const string INTERVENTION_NAME_KEY = "Intervention";
        private const string INTERVENTION_HANDLER_NAME_KEY = "intervention handler name";
        private const string INTERVENTION_NAME = "any intervention";
        private const string PARAMETER_NAME = "any parameter name";
        private const string PARAMETER_VALUE = "any parameter value";
    }

    internal class TestException : Exception
    {

    }
}
