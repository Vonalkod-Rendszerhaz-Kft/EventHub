using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vrh.XmlProcessing;
using System.Configuration;

namespace Vrh.EventHub.Protocols.InsideApplication
{
    internal class EventHubInsideApplicationConfig : LinqXMLProcessorBase
    {
        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="parameterFile">XML fájl aminek a feldolgozására az osztály készül</param>
        internal EventHubInsideApplicationConfig(string parameterFile)
        {
            _xmlFileDefinition = parameterFile;
        }

        #region Retrive all information from XML

        /// <summary>
        /// A csatorna timeoutja
        /// </summary>
        internal TimeSpan ChannelTimeout
        {
            get
            {
                string timoutStr = ConfigurationManager.AppSettings[InsideApplicationChannel.MODUL_PREFIX + ":" + Me.ChannelTimoutElement.NAME];
                TimeSpan timeout = new TimeSpan(0, 0, 1);
                if (!TimeSpan.TryParse(timoutStr, out timeout))
                {
                    timeout = GetElementValue<TimeSpan>(GetXElement(Me.ChannelTimoutElement.NAME), new TimeSpan(0, 0, 1));
                }
                return timeout;
            }
        }

        #endregion Retrive all information from XML

        #region Defination of namming rules in XML

        internal static class Me
        {
            internal static class ChannelTimoutElement
            {
                internal const string NAME = "ChannelTimout";
            }
        }

        #endregion Defination of namming rules in XML
    }
}
