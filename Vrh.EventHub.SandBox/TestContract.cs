using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vrh.EventHub.Core.Interfaces;

namespace Vrh.EventHub.SandBox
{
    [ContractMeta("This is a test contract for Event hub")]
    public class TestContract : BaseContract
    {
        public TestContract()
        {
            //DefaultCallBack = new DefaultCallBack();
            //CountProduct = new CountProduct();
        }

        public DefaultCallBack DefaultCallBack { get; set; }

        public CountProduct CountProduct { get; set; }
    }

    [MessageMeta("Default response message")]
    public class DefaultCallBack : BaseMessage
    {
        public bool IsOK { get; set; }

        public string Message { get; set; }
    }

    [MessageMeta("Increase Count of production", typeof(DefaultCallBack))]
    public class CountProduct : BaseMessage
    {
        public string ProductItem { get; set; }

        public int Qty { get;  set; }
    }
}

