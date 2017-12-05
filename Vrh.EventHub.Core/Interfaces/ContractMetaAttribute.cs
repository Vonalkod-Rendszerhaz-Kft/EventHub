using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vrh.EventHub.Core.Interfaces
{
    /// <summary>
    /// Atribute for Contract class defination
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ContractMetaAttribute : Attribute
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="description">description of message</param>
        public ContractMetaAttribute(string description)
        {
            _description = description;
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
        /// description of this messsage
        /// </summary>
        private string _description;
    }
}
