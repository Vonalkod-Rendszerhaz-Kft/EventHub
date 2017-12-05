using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vrh.EventHub.Core.Interfaces
{
    /// <summary>
    /// Egy üzenetetet reprezentáló entitás definiciója
    /// </summary>
    public interface IMessage
    {
        /// <summary>
        /// Az üzenet string azonosítója
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Az üzenet emberi fogyasztásra alkalmas leírása
        /// </summary>
        string Description { get; }

        /// <summary>
        /// A feldolgozáskor visszahívandó üzenet
        /// </summary>
        Type CallbackMessage { get; }

        /// <summary>
        /// Ez egy callback üzenet
        /// </summary>
        bool IsCallback { get; }
    }
}
