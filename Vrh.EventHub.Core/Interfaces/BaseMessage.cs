using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vrh.EventHub.Core.Interfaces
{
    /// <summary>
    /// Alap message implementáció. Minden Eventhub message.nek eből kell származnia!!!
    /// </summary>
    public class BaseMessage : IMessage
    {
        /// <summary>
        /// A callback return message-t tárolja.
        /// </summary>
        public Type CallbackMessage
        {
            get
            {
                Type returnMessage = null;
                MessageMetaAttribute attribute = (MessageMetaAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(MessageMetaAttribute));
                if (attribute != null)
                {
                    returnMessage = attribute.CallbackMessage;
                }
                return returnMessage;
            }
        }

        /// <summary>
        /// A messagehez tartozó emberi leírás
        /// </summary>
        public string Description
        {
            get
            {
                string description = this.GetType().FullName;
                MessageMetaAttribute attribute = (MessageMetaAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(MessageMetaAttribute));
                if (attribute != null)
                {
                    description = attribute.Description;
                }
                return description;
            }
        }

        /// <summary>
        /// Jelzi, hogy ez egy callback üzenet
        /// </summary>
        public bool IsCallback
        {
            get
            {
                bool isCallback = false;
                MessageMetaAttribute attribute = (MessageMetaAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(MessageMetaAttribute));
                if (attribute != null)
                {
                    isCallback = attribute.IsCallback;
                }
                return isCallback;
            }
        }

        /// <summary>
        /// Az üzenethez tartózó kulcs
        /// </summary>
        public string Id
        {
            get
            {
                return this.GetType().Name;
            }
        }
    }
}
