using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vrh.EventHub.Core.Exceptions;

namespace Vrh.EventHub.Core.Interfaces
{
    /// <summary>
    /// Alap (üres) emitter megvalósítás. Minden evnetHub emitter kötelezően ebből származik!!!
    /// </summary>
    public class BaseEmitter : IEmitter, IDisposable
    {
        /// <summary>
        /// Visszadja az Emitter típusát
        /// </summary>
        public string EmitterType
        {
            get
            {
                return this.GetType().FullName;
            }
        }

        /// <summary>
        /// Emitter azonosítója
        /// </summary>
        public string Id
        {
            get
            {
                return _id;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    EventHubException.ThrowOrNo(new EmptyEmitterIdNotPosibleException(), this.GetType());
                }
                if (_id != null)
                {
                    EventHubException.ThrowOrNo(new EmitterIdAlreadySettedException(), this.GetType());
                }
                _id = value;
            }
        }

        /// <summary>
        /// Az emitter típushoz tartozó emberi fogyasztásra szánt információ
        /// </summary>
        public string Description
        {
            get
            {
                return _description;
            }
        }

        /// <summary>
        /// Elküldi a paraméterben kapott választ, ha van callback (visszatérő üzenet), megvárja és visszadja azt. (Ha nincs, akkor BaseCallbackMessage-et kell adni.)
        /// </summary>
        /// <param name="message">A küldendő üzenet.</param>
        /// <returns>A válasz</returns>
        public virtual IMessage SendMessage(IMessage message)
        {
            throw new NotImplementedException("Must override this!!!");
        }

        /// <summary>
        /// Bedobja a paraméterben kapott üzenetet a csatornára. A SendMessage aszinkron párja.
        /// </summary>
        /// <param name="message">A küldendő üzenet.</param>
        public virtual void DropMessage(IMessage message)
        {
            throw new NotImplementedException("Must override this!!!");
        }

        /// <summary>
        /// Felülírandó!!!
        /// </summary>
        public virtual void Dispose()
        {
            throw new NotImplementedException("Must override this!!!");
        }

        /// <summary>
        /// Az emitter azonosítója
        /// </summary>
        protected string _id = null;

        /// <summary>
        /// Az emitter típushoz tartozó emberi fogyasztásra szánt információ (Adj neki értéket a leszármazottban!)
        /// </summary>
        protected string _description = null;
    }
}
