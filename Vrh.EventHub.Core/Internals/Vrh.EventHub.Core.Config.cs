using System;
using System.Configuration;
using Vrh.XmlProcessing;

namespace Vrh.EventHub.Core
{
    /// <summary>
    /// Az EventHub konfigurációját hordozó (kiolvasó) típus
    /// </summary>
    internal class EventHubCoreConfig : LinqXMLProcessorBase
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
                string throwStr = ConfigurationManager.AppSettings[EventHubCore.MODUL_PREFIX + ":" + Me.ThrowEventHubCoreExceptionsElement.NAME];
                bool throwExceptions = false;
                if (!Boolean.TryParse(throwStr, out throwExceptions))
                {
                    throwExceptions = GetExtendedBoolElementValue(GetXElement(Me.ThrowEventHubCoreExceptionsElement.NAME), false, "1", "yes");
                }
                return throwExceptions;
            }
        }

        #endregion Retrive all information from XML

        #region Defination of namming rules in XML

        /// <summary>
        /// A konfigurációs XML leírása
        /// </summary>
        internal static class Me
        {
            /// <summary>
            /// ThrowEventHubCoreExceptions XML element
            /// </summary>
            internal static class ThrowEventHubCoreExceptionsElement
            {
                /// <summary>
                /// A lehetséges Redis connection-öket felsoroló XML element neve
                /// </summary>
                internal const string NAME = "ThrowEventHubCoreExceptions";
            }
        }

        #endregion Defination of namming rules in XML
    }
}
