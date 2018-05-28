using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vrh.EventHub.Core;
using Vrh.EventHub.Protocols.RedisPubSub;

namespace Endpoint1
{
    class Program
    {
        static AutoResetEvent _waitForTestRuns = new AutoResetEvent(false);


        static private int _round = 0;

        static private int _testruns = 0;

        static object _staticlocker = new object();

        static void Main(string[] args)
        {
            EventHubCore.InitielizeChannel<RedisPubSubChannel>(TestConctract.CHANNEL_ID);
            do
            {
                //Console.Clear();
                Console.WriteLine("Number of test runs (leave empty for exit):");
                string strNum = Console.ReadLine();
                if (String.IsNullOrEmpty(strNum))
                {
                    return;
                }
                else
                {
                    int.TryParse(strNum, out _testruns);
                }
                _round = 0;
                for (int i = 0; i < _testruns; i++)
                {
                    Task.Factory.StartNew(() => RunTest(), TaskCreationOptions.LongRunning);
                }
                _waitForTestRuns.WaitOne();
                Console.WriteLine($"All tests {_testruns} passed");
                Console.WriteLine($"Press a key for new round!");
                Console.ReadKey();
                _round = 0;
                _testruns = 0;
            } while (true);

            string exit = String.Empty;
            while (exit != "e")
            {
                Warmup();
                Thread.Sleep(2000);
                SyncCallThrougput();
                Thread.Sleep(2000);
                AsyncCallThroughputWithEndSignal();
                Thread.Sleep(2000);
                FullAsyncCallThroughputWithEndSignal();
                exit = Console.ReadLine();
                Write("");
            }
        }

        async static void RunTest()
        {
            lock (_staticlocker)
            {
                Console.WriteLine($"rounds: { _round }");
                var twoNumber = new TestConctract.TwoNumber
                {
                    One = 1,
                    Two = 1,
                    Count = _testruns,
                    No = _round,
                };
                try
                {
                    lock (_staticlocker)
                    {
                        Console.WriteLine($"CALL:............{twoNumber.No}");
                        var result = EventHubCore.Call<RedisPubSubChannel,
                            TestConctract.TwoNumber,
                            TestConctract.Result>(TestConctract.CHANNEL_ID, twoNumber, new TimeSpan(0, 0, 2));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!{ex.Message}");
                }
                _round++;
                if (_round == _testruns)
                {
                    _waitForTestRuns.Set();
                }
            }
        }

        public static void Warmup()
        {
            Write("Warmup-----------------------------------------------------");
            DateTime startTime = DateTime.UtcNow;
            var twoNumber = new TestConctract.TwoNumber
            {
                One = 1,
                Two = 1,
                Count = TestConctract.TEST_ROUNDS,
                No = 0
            };
            EventHubCore.Send<RedisPubSubChannel,
                TestConctract.TwoNumber
                >(TestConctract.CHANNEL_ID, twoNumber);
            DateTime endTime = DateTime.UtcNow;
            var cost = endTime.Subtract(startTime).TotalMilliseconds;
            Write($"Warmup cost: {cost} millisec");
        }

        private static void SyncCallThrougput()
        {
            Write("TestRun #1: Szinkron átbocsátás-----------------------------------------------------");
            DateTime startTime = DateTime.UtcNow;
            for (int i = 0; i < TestConctract.TEST_ROUNDS; i++)
            {
                var twoNumber = new TestConctract.TwoNumber
                {
                    One = 1,
                    Two = i,
                    Count = TestConctract.TEST_ROUNDS,
                    No = i
                };
                var result = EventHubCore.Call<RedisPubSubChannel,
                    TestConctract.TwoNumber,
                    TestConctract.Result>(TestConctract.CHANNEL_ID, twoNumber);
                NonBlockWrite($"send: { twoNumber.One } + { twoNumber.Two }; receive: { result.ResultValue }");
            }
            DateTime endTime = DateTime.UtcNow;
            var cost = endTime.Subtract(startTime).TotalMilliseconds;
            var throughputpermin = ((double)TestConctract.TEST_ROUNDS * 60 * 1000) / cost;
            var throughputpersec = throughputpermin / 60;
            Write($"{TestConctract.TEST_ROUNDS} üzenet {cost} millisecundum alatt, átbcsáttás: {throughputpermin} üzenet/perc ({throughputpersec} üzenet/sec)");
        }

        private static void AsyncCallThroughputWithEndSignal()
        {
            Write("TestRun #2: Aszinkron átbocsátás-----------------------------------------------------");
            EventHubCore.RegisterHandler<RedisPubSubChannel, TestConctract.AllOk>(TestConctract.CHANNEL_ID, AllOk);
            EventHubCore.Send<RedisPubSubChannel, TestConctract.StartNewRound>(TestConctract.CHANNEL_ID, new TestConctract.StartNewRound());
            if (!WaitAllReceiveSemafor.WaitOne(TestConctract.TEST_ROUNDS * 1000))
            {
                Write("!!!!!!!!!!!!!!!!!!!!!!!!!           Receive timout occured         !!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            }
            DateTime startTime = DateTime.UtcNow;
            for (int i = 0; i < TestConctract.TEST_ROUNDS; i++)
            {
                EventHubCore.Send<RedisPubSubChannel, TestConctract.TestMessage>(TestConctract.CHANNEL_ID, new TestConctract.TestMessage { Count = TestConctract.TEST_ROUNDS, No = i });
            }
            DateTime middleTime = DateTime.UtcNow;
            var cost = middleTime.Subtract(startTime).TotalMilliseconds;
            var throughputpermin = ((double)TestConctract.TEST_ROUNDS * 60 * 1000) / cost;
            var throughputpersec = throughputpermin / 60;
            Write($"{TestConctract.TEST_ROUNDS} üzenet elküldve {cost} milliszekundum alatt; küldő oldali teljes aszinkron átbocsátás: {throughputpermin} üzenet/min ({throughputpersec} üzenet/sec)");
            if (!WaitAllReceiveSemafor.WaitOne(TestConctract.TEST_ROUNDS * 1000))
            {
                Write("!!!!!!!!!!!!!!!!!!!!!!!!!           Receive timout occured         !!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            }
            DateTime endTime = DateTime.UtcNow;
            cost = endTime.Subtract(startTime).TotalMilliseconds;
            throughputpermin = ((double)TestConctract.TEST_ROUNDS * 60 * 1000) / cost;
            throughputpersec = throughputpermin / 60;
            Write($"{TestConctract.TEST_ROUNDS} üzenet elküldve és fogadó oldalon mind fogadva {cost} milliszekundum alatt; kétoldali aszinkron átbocsátás: {throughputpermin} üzenet/min ({throughputpersec} üzenet/sec)");
            EventHubCore.DropHandler<RedisPubSubChannel, TestConctract.AllOk>(TestConctract.CHANNEL_ID, AllOk);
        }

        private static void FullAsyncCallThroughputWithEndSignal()
        {
            Write("TestRun #3: Teljesen aszinkron átbocsátás (aszinkron is küldve)-----------------------------------------------------");
            EventHubCore.RegisterHandler<RedisPubSubChannel, TestConctract.AllOk>(TestConctract.CHANNEL_ID, AllOk);
            EventHubCore.Send<RedisPubSubChannel, TestConctract.StartNewRound>(TestConctract.CHANNEL_ID, new TestConctract.StartNewRound());
            if (!WaitAllReceiveSemafor.WaitOne(TestConctract.TEST_ROUNDS * 1000))
            {
                Write("!!!!!!!!!!!!!!!!!!!!!!!!!           Receive timout occured         !!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            }
            DateTime startTime = DateTime.UtcNow;
            for (int i = 0; i < TestConctract.TEST_ROUNDS; i++)
            {
                EventHubCore.SendAsync<RedisPubSubChannel, TestConctract.TestMessage>(TestConctract.CHANNEL_ID, new TestConctract.TestMessage { Count = TestConctract.TEST_ROUNDS, No = i });
                //AsyncTestMessageSender(new TestConctract.TestMessage { Count = TestConctract.TEST_ROUNDS, No = i });
            }
            DateTime middleTime = DateTime.UtcNow;
            var cost = middleTime.Subtract(startTime).TotalMilliseconds;
            var throughputpermin = ((double)TestConctract.TEST_ROUNDS * 60 * 1000) / cost;
            var throughputpersec = throughputpermin / 60;
            Write($"{TestConctract.TEST_ROUNDS} üzenet elküldve {cost} milliszekundum alatt; küldő oldali teljes aszinkron átbocsátás: {throughputpermin} üzenet/min ({throughputpersec} üzenet/sec)");
            if (!WaitAllReceiveSemafor.WaitOne(TestConctract.TEST_ROUNDS * 1000))
            {
                Write("!!!!!!!!!!!!!!!!!!!!!!!!!           Receive timout occured         !!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            }
            DateTime endTime = DateTime.UtcNow;
            cost = endTime.Subtract(startTime).TotalMilliseconds;
            throughputpermin = ((double)TestConctract.TEST_ROUNDS * 60 * 1000) / cost;
            throughputpersec = throughputpermin / 60;
            Write($"{TestConctract.TEST_ROUNDS} üzenet elküldve és fogadó oldalon mind fogadva {cost} milliszekundum alatt; kétoldali aszinkron átbocsátás: {throughputpermin} üzenet/min ({throughputpersec} üzenet/sec)");
            EventHubCore.DropHandler<RedisPubSubChannel, TestConctract.AllOk>(TestConctract.CHANNEL_ID, AllOk);
        }

        private static void AsyncTestMessageSender(TestConctract.TestMessage message)
        {
            Task.Run(() =>
                {
                    EventHubCore.Send<RedisPubSubChannel, TestConctract.TestMessage>(TestConctract.CHANNEL_ID, message);
                });
            NonBlockWrite($"{message.No} számú üzenet elküldve");
        }


        public static AutoResetEvent WaitAllReceiveSemafor { get; } = new AutoResetEvent(false);

        private static void AllOk(TestConctract.AllOk obj)
        {
            WaitAllReceiveSemafor.Set();
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

        private static object _consoleLocker = new object();
    }

    public class TestConctract
    {
        public class TestMessage
        {
            public int Count { get; set; }

            public int No { get; set; }
        }

        public class TwoNumber : TestMessage
        {
            public int One { get; set; }

            public int Two { get; set; }
        }

        public class Result : TestMessage
        {
            public int ResultValue { get; set; }
        }

        public class AllOk
        {
        }

        public class StartNewRound
        {
        }

        public const string CHANNEL_ID = "test";

        public const int TEST_ROUNDS = 100000;
    }
}
