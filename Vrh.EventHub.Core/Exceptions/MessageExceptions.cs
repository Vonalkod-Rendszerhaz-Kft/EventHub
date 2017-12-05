using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vrh.EventHub.Core.Exceptions
{
    /// <summary>
    /// Kivétel, ha használni kívánt üzenet nem BaseMessage leszármazott
    /// </summary>
    public class BadMessageTypeException : FatalEventHubException
    {
        /// <summary>
        /// Construktor
        /// </summary>
        public BadMessageTypeException() : base("All Event Hub message must derived by BaseMessage!")
        {
        }
    }
}
