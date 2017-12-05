using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vrh.EventHub.Core.Interfaces
{
    /// <summary>
    /// Egy szerződés, amely definiálja, hogy az adott típusú csatornán milyen üzenetek közlekedhetnek
    /// </summary>
    public interface IContract
    {
        /// <summary>
        /// A szerződés azonosítója
        /// </summary>
        string Id { get; }

        /// <summary>
        /// A szerződés verziója
        /// </summary>
        string Version { get; }

        /// <summary>
        /// A szerződés megnevezése
        /// </summary>
        string Description { get; }

        /// <summary>
        /// A szerződést definiáló assembly neve
        /// </summary>
        string ContractAssembly { get; }

        /// <summary>
        /// Kezelt üzenetek listája
        /// </summary>
        List<MessageWithCallback> HandledMessages { get; }

        /// <summary>
        /// A szerződésben szereplő üzenetek nevei
        /// </summary>
        List<string> HandledMessageNames { get; }

        /// <summary>
        /// A szerződésben szereplő callback üzenetek nevei
        /// </summary>
        List<string> HandledCallbackNames { get; }

        /// <summary>
        /// Visszaad egy üres példányt a megadott üzenettípusból, amit ezután ki kell tölteni a küldéshez
        ///  A lekért üzenetnek message típusónak kell lennie, callback nem lehet!
        /// </summary>
        /// <typeparam name="T">az üzenet amiből a példányt kérjük</typeparam>
        /// <returns>üzenet példány</returns>
        IMessage GetEmptyMessageInstance<T>() where T : BaseMessage, new();

        /// <summary>
        /// Visszaad egy üres példányt a megadott üzenettípusból, amit ezután ki kell tölteni a küldéshez
        ///  A lekért üzenet csak callback lehet!
        /// </summary>
        /// <typeparam name="T">az üzenet amiből a példányt kérjük</typeparam>
        /// <returns>üzenet példány</returns>
        IMessage GetEmptyCallbackInstance<T>() where T : BaseMessage, new();
    }

    /// <summary>
    /// Üzenet a callback párjával
    /// </summary>
    public class MessageWithCallback
    {
        /// <summary>
        /// Üzenet neve
        /// </summary>
        public string MessageNema { get; set; }

        /// <summary>
        /// Callback neve
        /// </summary>
        public string CallbackName { get; set; }

        /// <summary>
        /// TosString ovverride
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{this.MessageNema} --> {this.CallbackName}";
        }
    }
}
