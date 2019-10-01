using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Vrh.EventHub.Core;
using Vrh.EventHub.Protocols.RedisPubSub;

namespace EventhubUseSamples
{
    class Program
    {
        static void Main(string[] args)
        {
            var am = new AddMachine();
            var amc = new AddMachineClient();
            //amc.TestAsyncSend();
            //Thread.Sleep(1000);
            //Console.WriteLine("");
            amc.TestSyncCall();
            //Thread.Sleep(1000);
            //Console.WriteLine("");
            //amc.TestAsyncSendWithResultHandling();
            Console.ReadLine();
        }

        public static void NonBlockWrite(string line)
        {
            Task.Run(() => Write(line));
        }

        public static void Write(string line)
        {
            lock (_consoleLocker)
            {
                Console.WriteLine(line);
            }
        }

        private static object _consoleLocker = new object();
    }

    public static class Extensions
    {
        public static string Print(this List<int> intList)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var listItem in intList)
            {
                sb.AppendFormat("{0} | ", listItem);
            }
            string result = sb.ToString();
            return result.Length > 3 ?
                result.Remove(result.Length - 3, 3) : result;
        }
    }


    public class AddMachine
    {
        public AddMachine()
        {
            // Aszinkron válasz nélkül
            EventHubCore.RegisterHandler<RedisPubSubChannel, AddThis>("addmachine", AddAndNothing, true);            
            // A hívó oldal call-t fog hívni, és a válasz közvetlenül visszamegy a hívó oldalra
            EventHubCore.RegisterHandler<RedisPubSubChannel, AddThis, Result>("addmachine", AddForSyncUse);
            // Aszinkron az EventHub.Core Request-Response szolgáltatásainak kihasználásával
            EventHubCore.RegisterHandler<RedisPubSubChannel, Request<AddThis, Result>>("addmachine", AddForAsyncUse, true);
            // Aszinkron egyedi megvalósítással
            //
        }

        public void AddAndNothing(AddThis forAdding)
        {
            AddAll(forAdding);
            Program.Write($"AddMachine side: receive this (across handler for async (void), AddThis: {forAdding.Numbers.Print()}");
        }

        public Response<Result> AddForSyncUse(Request<AddThis, Result> addingRequest)
        {
            Program.Write($"AddMachine side: receive this (across handler for sync AddThis, Result handler: {addingRequest.RequestContent.Numbers.Print()}");
            int result = AddAll(addingRequest.RequestContent);
            var myResponse = addingRequest.MyResponse;
            myResponse.ResponseContent = new Result { Amount = result };
            Program.Write($"AddMachine side: send result (sync) with this data: {myResponse.ResponseContent.Amount}");
            return myResponse;
        }

        public void AddForAsyncUse(Request<AddThis, Result> addingRequest)
        {
            Program.Write($"AddMachine side: receive this (across handler for async AddThis, void handler: {addingRequest.RequestContent.Numbers.Print()}");
            int result = AddAll(addingRequest.RequestContent);
            var myResponse = addingRequest.MyResponse;
            myResponse.ResponseContent = new Result { Amount = result };
            Program.Write($"AddMachine side: Send result async (Response<Result>) to client side (for request id: {myResponse.RequestId}), with this data: {myResponse.ResponseContent.Amount}");
            EventHubCore.Send<RedisPubSubChannel, Response<Result>>("addmachine", myResponse);
        }

        private int AddAll(AddThis forAdding)
        {
            int result = 0;
            foreach (var number in forAdding.Numbers)
            {
                result += number;
            }
            return result;
        }
    }


    public class AddMachineClient
    {
        public void TestAsyncSend()
        {
            Program.Write("Sample #1: Async Send -------------------------------------------------------------------------");
            var adt = new AddThis { Numbers = _numbers };
            Program.Write($"Client side: send this with async Send<T> where T is AddThis type: {adt.Numbers.Print()}");
            EventHubCore.Send<RedisPubSubChannel, AddThis>("addmachine", adt);
        }

        public void TestSyncCall()
        {
            Program.Write("Sample #2: Sync Call with Response -------------------------------------------------------------------------");
            var adt = new AddThis { Numbers = _numbers };
            Program.Write($"Client side: send this with sync Call<TReq, TResp>, where TReq is AddThis type and TResp is Result type. Sended: {adt.Numbers.Print()}");
            var result = EventHubCore.Call<RedisPubSubChannel, AddThis, Result>("addmachine", adt);
            Program.Write($"Client side: Received result (in Result type): {result.Amount}");
        }

        public void TestAsyncSendWithResultHandling()
        {
            Program.Write("Sample #3: Async Send with Response handling -------------------------------------------------------------------------");
            // Mindig először az eseménykezelőt regisztráljuk, ami a választ foigja fogadni!!!
            EventHubCore.RegisterHandler<RedisPubSubChannel, Result>("addmachine", ReceiveAddMachineAsyncResult, true);
            // Összeállítja a Request<T, T> példányt, amelyik a kérést reprezentálja    
            var request = new Request<AddThis, Result>();
            request.RequestContent = new AddThis { Numbers = _numbers };
            EventHubCore.Send<RedisPubSubChannel, Request<AddThis, Result>>("addmachine", request);
            Program.Write($"Client side: Send async request (Request<AddThis, Result>) with this data: {request.RequestContent.Numbers.Print()}; request Id: {request.Id}");
        }

        public void ReceiveAddMachineAsyncResult(Response<Result> response)
        {
            Program.Write($"Client side: Receive async response (Response<Result>) for {response.RequestId} request, with this data: {response.ResponseContent.Amount}");
        }


        private List<int> _numbers
        {
            get
            {
                var rnd = new Random();
                var numbers = new List<int>();
                for (int i = 0; i < rnd.Next(8) + 2; i++)
                {
                    numbers.Add(rnd.Next(255));
                }
                return numbers;
            }
        }
    }


    public class AddMachineContract
    {
        public class AddThis
        {
            public List<int> Numbers { get; set; }
        }
        
        public class Result
        {
            public int Amount { get; set; }
        }

        public const string CAHNEL_ID = "addmachine";        
    }

    public class AddThis
    {
        public List<int> Numbers { get; set; }
    }

    public class Result
    {
        public int Amount { get; set; }
    }

}
