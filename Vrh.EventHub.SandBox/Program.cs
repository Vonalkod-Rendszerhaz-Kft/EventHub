using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vrh.EventHub.Core;
using Vrh.EventHub.Core.Interfaces;

namespace Vrh.EventHub.SandBox
{
    class Program
    {
        static void Main(string[] args)
        {
            var cp = new CountProduct()
            {
                ProductItem = "ABC",
                Qty = 5,
            };

            TestContract tc = new TestContract();
            var hm = tc.HandledMessages;
            

            Console.WriteLine(cp.CallbackMessage.Name);
            Console.WriteLine(cp.Description);
            Console.WriteLine(cp.Id);

            Console.ReadLine();           
        }
    }
}
