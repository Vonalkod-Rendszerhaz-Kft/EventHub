using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vrh.EventHub.Core;
using Vrh.EventHub.Protocols.RedisPubSub;
using Endpoint1;

namespace Endpoint2
{
    class Program
    {
        static void Main(string[] args)
        {
            DateTime startTime = DateTime.UtcNow;
            EventHubCore.RegisterHandler<RedisPubSubChannel, TestConctract.TwoNumber, TestConctract.Result>(TestConctract.CHANNEL_ID, Add);
            DateTime endTime = DateTime.UtcNow;
            var cost = endTime.Subtract(startTime).TotalMilliseconds;
            Write($"1. handler bejegyzési ideje: {cost} millisec");

            startTime = DateTime.UtcNow;
            EventHubCore.RegisterHandler<RedisPubSubChannel, TestConctract.StartNewRound>(TestConctract.CHANNEL_ID, ReceiveStartNewRound);
            endTime = DateTime.UtcNow;
            cost = endTime.Subtract(startTime).TotalMilliseconds;
            Write($"2. handler bejegyzési ideje: {cost} millisec");

            startTime = DateTime.UtcNow;
            EventHubCore.RegisterHandler<RedisPubSubChannel, TestConctract.TestMessage>(TestConctract.CHANNEL_ID, ReceiveTestMessage);
            endTime = DateTime.UtcNow;
            cost = endTime.Subtract(startTime).TotalMilliseconds;
            Write($"3. handler bejegyzési ideje: {cost} millisec");

            Write("Fogadó oldal kész a tesztre.");
            string exit = String.Empty;
            while (exit != "e")
            {
                lock (_receivedLocker)
                {
                    Write($"Fogadott {_received} db üzenet");
                }
                exit = Console.ReadLine();
            }
            Console.ReadLine();
        }

        private static void ReceiveStartNewRound(TestConctract.StartNewRound obj)
        {
            NonBlockWrite("Új kör:");
            lock (_receivedLocker)
            {
                _received = 0;
                _batchReceiveStarted = DateTime.UtcNow;
            }
            EventHubCore.Send<RedisPubSubChannel, TestConctract.AllOk>(TestConctract.CHANNEL_ID, new TestConctract.AllOk());
        }

        public static Response<TestConctract.Result> Add(Request<TestConctract.TwoNumber, TestConctract.Result> request)
        {
            var myResponse = request.MyResponse;
            var input = request.RequestContent;
            myResponse.ResponseContent = new TestConctract.Result { Count = input.Count, No = input.No, ResultValue = input.One + input.Two };
            NonBlockWrite($"Receive: { input.No }");
            return myResponse;
        }

        public static void ReceiveTestMessage(TestConctract.TestMessage message)
        {
            lock (_receivedLocker)
            {
                _received++;
                if (_received == TestConctract.TEST_ROUNDS)
                {
                    EventHubCore.Send<RedisPubSubChannel, TestConctract.AllOk>(TestConctract.CHANNEL_ID, new TestConctract.AllOk());
                    DateTime endTime = DateTime.UtcNow;
                    var cost = endTime.Subtract(_batchReceiveStarted).TotalMilliseconds;
                    var throughputpermin = ((double)TestConctract.TEST_ROUNDS * 60 * 1000) / cost;
                    var throughputpersec = throughputpermin / 60;
                    Write($"{TestConctract.TEST_ROUNDS} db üzenet fogadva, és feldolgozva {cost} milliszekundum alatt; fogadási átbocsátás: {throughputpermin} üzenet/min ({throughputpersec} üzenet/sec)");
                }
            }
            NonBlockWrite($"Received {_received} db (most érkezett a { message.No } sorszámú)");
        }

        private static void NonBlockWrite(string line)
        {
            // Teljesítmény mérésnél vedd ki kommentből a returnt, 
            //  hogy ne mérd bele a szinkronizált Console.Write-ok költségét
            // Ha a közbeni müködés részleteit is látni akarod consolüzenetekben, akkor commentezd a returnt.
            return;           
            Task.Run(() => Write(line));
        }

        private static void Write(string line)
        {
            lock (_consoleLocker)
            {
                Console.WriteLine(line);
            }
        }

        private static DateTime _batchReceiveStarted;

        private volatile static int _received = 0;

        private static object _receivedLocker = new object();

        private static object _consoleLocker = new object();
    }
}
