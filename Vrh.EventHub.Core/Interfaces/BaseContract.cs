using System;
using System.Collections.Generic;
using System.Linq;
using VRH.Common;
using Vrh.EventHub.Core.Exceptions;

namespace Vrh.EventHub.Core.Interfaces
{
    /// <summary>
    /// Szerződés ősosztály minden használt szerződést eből kell származtatni!
    /// </summary>
    public class BaseContract : IContract
    {
        /// <summary>
        /// A szerződés azonosítója
        /// </summary>
        public string Id
        {
            get
            {
                return this.GetType().FullName;
            }
        }

        /// <summary>
        /// A szerződés verziója
        /// </summary>
        public string Version
        {
            get
            {
                return this.GetType().Assembly.Version();
            }
        }

        /// <summary>
        /// A szerződés emberi fogyasztásra szánt leírása
        /// </summary>
        public string Description
        {
            get
            {
                string description = this.GetType().FullName;
                ContractMetaAttribute attribute = (ContractMetaAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(ContractMetaAttribute));
                if (attribute != null)
                {
                    description = attribute.Description;
                }
                return description;
            }
        }

        /// <summary>
        /// A szerződést definiáló assembly neve
        /// </summary>
        public string ContractAssembly
        {
            get
            {
                return this.GetType().Assembly.Location;
            }
        }

        /// <summary>
        /// A szerződésben definiált üzenetek neveinek listája (a callback üzenetek nem számítanak bele)
        /// </summary>
        public List<MessageWithCallback> HandledMessages
        {
            get
            {
                var returnList = new List<MessageWithCallback>();
                foreach (var msg in HandledMessagesAsType())
                {
                    var msgwclb = new MessageWithCallback()
                    {
                        MessageNema = msg.Name,
                    };
                    IMessage msgInstance = (IMessage)msg.GetConstructor(Type.EmptyTypes).Invoke(null);
                    msgwclb.CallbackName = msgInstance.CallbackMessage.Name;
                    returnList.Add(msgwclb);
                }
                return returnList;
            }
        }

        /// <summary>
        /// A szerződésben szereplő üzenetek nevei
        /// </summary>
        public List<string> HandledMessageNames
        {
            get
            {
                var returnList = new List<string>();
                foreach (var msg in HandledMessagesAsType())
                {
                    returnList.Add(msg.Name);
                }
                return returnList;
            }
        }

        /// <summary>
        /// A szerződésben szereplő callback üzenetek nevei
        /// </summary>
        public List<string> HandledCallbackNames
        {
            get
            {
                var returnList = new List<string>();
                foreach (var msg in HandledMessagesAsType(true))
                {
                    returnList.Add(msg.Name);
                }
                return returnList;
            }
        }

        /// <summary>
        /// Visszaad egy üres példányt a megadott üzenettípusból, amit ezután ki kell tölteni a küldéshez
        ///  A lekért üzenetnek message típusónak kell lennie, callback nem lehet!
        /// </summary>
        /// <typeparam name="T">az üzenet amiből a példányt kérjük</typeparam>
        /// <returns>üzenet példány</returns>
        public IMessage GetEmptyMessageInstance<T>() where T : BaseMessage, new()
        {
            if (!HandledMessagesAsType().Any(x => x.FullName == typeof(T).FullName))
            {
                EventHubException.ThrowOrNo(new ContractNotContainsException<T>(this.GetType()), this.GetType());
            }
            return new T();
        }

        /// <summary>
        /// Visszaad egy üres példányt a megadott üzenettípusból, amit ezután ki kell tölteni a küldéshez
        ///  A lekért üzenet csak callback lehet!
        /// </summary>
        /// <typeparam name="T">az üzenet amiből a példányt kérjük</typeparam>
        /// <returns>üzenet példány</returns>
        public IMessage GetEmptyCallbackInstance<T>() where T : BaseMessage, new()
        {
            if (!HandledMessagesAsType(true).Any(x => x.FullName == typeof(T).FullName))
            {
                EventHubException.ThrowOrNo(new ContractNotContainsException<T>(this.GetType()), this.GetType());
            }
            return new T();
        }

        /// <summary>
        /// A szerződésben definiált üzenetek listája (a callback üzenetek nem számítanak bele)
        /// </summary>
        private List<Type> HandledMessagesAsType(bool callbacks = false)
        {
            List<Type> returnList = new List<Type>();
            foreach (var prop in this.GetType().GetProperties())
            {
                var t = prop.PropertyType.GetInterface(typeof(IMessage).FullName);
                if (prop.PropertyType.GetInterface(typeof(IMessage).FullName) != null)
                {
                    MessageMetaAttribute attribute = (MessageMetaAttribute)Attribute.GetCustomAttribute(prop.PropertyType, typeof(MessageMetaAttribute));
                    if (attribute != null)
                    {
                        if (!callbacks)
                        {
                            if (attribute.CallbackMessage != null || !attribute.IsCallback)
                            {
                                returnList.Add(prop.PropertyType);
                            }
                        }
                        else
                        {
                            if (attribute.IsCallback)
                            {
                                returnList.Add(prop.PropertyType);
                            }
                        }
                    }
                }
            }
            return returnList;
        }

        /// <summary>
        /// A szerződésre kapcsolt kibocsátók
        /// </summary>
        protected List<IEmitter> _conectedEmitters;
    }
}
