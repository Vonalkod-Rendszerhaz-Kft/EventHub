using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vrh.EventHub.Core.Exceptions;

namespace Vrh.EventHub.Core.Interfaces
{
    /// <summary>
    /// Atribute for Message class defination
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class MessageMetaAttribute : Attribute
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="description">description of message</param>
        /// <param name="callbackMessage">callback (response) message </param>
        public MessageMetaAttribute(string description, Type callbackMessage = null, bool isCallback = true)
        {
            if (callbackMessage != null && callbackMessage.BaseType != typeof(BaseMessage))
            {
                EventHubException.ThrowOrNo(new BadMessageTypeException(), this.GetType());
            }
            _description = description;
            _callbackMessage = callbackMessage;
            _isCallback = callbackMessage == null && isCallback;
        }

        /// <summary>
        /// Description for this message
        /// </summary>
        public string Description
        {
            get
            {
                return _description;
            }
        }

        /// <summary>
        /// Response message for this message if exists
        /// </summary>
        public Type CallbackMessage
        {
            get
            {
                return _callbackMessage;
            }
        }

        /// <summary>
        /// This is a callback message 
        /// </summary>
        public bool IsCallback
        {
            get
            {
                return _isCallback;
            }
        }

        /// <summary>
        /// description of this messsage
        /// </summary>
        private string _description;

        /// <summary>
        /// callback of this message if exists
        /// </summary>
        private Type _callbackMessage;

        /// <summary>
        /// This is a callbackmessage
        /// </summary>
        private bool _isCallback;
    }
}
