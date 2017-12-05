using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vrh.EventHub.Core.Interfaces
{
    /// <summary>
    /// Egy küldőt reprezentál a csatornán
    /// </summary>
    public interface IEmitter
    {
        /// <summary>
        /// Az emitter típusa
        /// </summary>
        string EmitterType { get; }

        /// <summary>
        /// Az emitter azonosítója
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// Az emitter emberibb leírása
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Elküldi a paraméterben kapott választ, ha van callback (visszatérő üzenet), megvárja és visszadja azt. (Ha nincs, akkor BaseCallback-et ad.)
        /// </summary>
        /// <param name="message">A küldendő üzenet.</param>
        /// <returns>A válasz</returns>
        IMessage SendMessage(IMessage message);

        /// <summary>
        /// Bedobja a paraméterben kapott üzenetet a csatornára. A SendMessage aszinkron párja.
        /// </summary>
        /// <param name="message">A küldendő üzenet.</param>
        void DropMessage(IMessage message);
    }
}
