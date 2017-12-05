using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vrh.EventHub.Core.Exceptions
{
    /// <summary>
    /// A szerződés nem tartalmazza a megadott üzenetet
    /// </summary>
    /// <typeparam name="TContract">szerződés</typeparam>
    /// <typeparam name="TMessage">üzenet</typeparam>
    public class ContractNotContainsException<TMessage> : FatalEventHubException
    {
        public ContractNotContainsException(Type contract) : base($"This contract ({contract.FullName}) is not contain this message: {typeof(TMessage).FullName}")
        {
        }
    }
}
