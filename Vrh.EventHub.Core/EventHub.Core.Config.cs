using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vrh.LinqXMLProcessor.Base;

namespace Vrh.EventHub.Core
{
    internal class EventHubCoreConfig : LinqXMLProcessorBaseClass
    {
        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="parameterFile">XML fájl aminek a feldolgozására az osztály készül</param>
        internal EventHubCoreConfig(string parameterFile)
        {
            _xmlFileDefinition = parameterFile;
        }

        #region Retrive all information from XML

        /// <summary>
        /// Az eventhub dobja e az EventHubException-öket (true), vagy nyelje le őket (false)
        /// </summary>
        internal bool ThrowEventHubCoreExceptions
        {
            get
            {
                string throwStr = ConfigurationManager.AppSettings[MODUL_PREFIX + ":" + THROW_EVENTHUBCORE_EXCEPTIONS];
                bool throwExceptions = false;
                if (!Boolean.TryParse(throwStr, out throwExceptions))
                {
                    throwExceptions = GetExtendedBoolElementValue(GetXElement(THROW_EVENTHUBCORE_EXCEPTIONS), false, "1", "yes");
                }
                return throwExceptions;
            }
        }

        #endregion Retrive all information from XML

        #region Defination of namming rules in XML

        internal const string MODUL_PREFIX = "Vrh.EventHub.Core";
        private const string THROW_EVENTHUBCORE_EXCEPTIONS = "ThrowEventHubCoreExceptions";

        #endregion Defination of namming rules in XML
    }
}
