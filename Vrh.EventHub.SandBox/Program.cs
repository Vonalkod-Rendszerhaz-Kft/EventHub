using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Vrh.EventHub.Core;
using Vrh.EventHub.Protocols.RedisPubSub;
using Vrh.EventHub.Protocols.InsideApplication;
using Vrh.Logger;
using StackExchange.Redis;
using Newtonsoft.Json.Serialization;
using System.ComponentModel;

namespace Vrh.EventHub.SandBox
{
    class Program
    {
        static void Main(string[] args)
        {
            //var rp = new ReferencePingPong(100);

            //var p = new PingPong<InsideApplicationChannel>(100);
            //var p2 = new PingPong<RedisPubSubChannel>(100);

            var t = new Test();
            t.ErrorHandlingTest();

            Console.ReadLine();
            return;

            //var s = new SimpleCalculatorService();
            //var c = new CalculatorClient();
            //c.SincTest();

            //Console.ReadLine();
            //return;

            //Test t = new Test();
            //t.Init();
            //t.StartTest();

            //Console.ReadLine();
            //return;
        }

        public static void Write(string line)
        {
            lock (_consoleLocker)
            {
                Console.WriteLine(line);
            }
        }

        static object _consoleLocker = new object();
    }

    public class Test
    {
        public void StartTest()
        {
            TwoNumber m = new TwoNumber { One = 1, Two = 2 };
            //EventHubCore.Send<RedisPubSubChannel, TwoNumber>("testchannel", new Request<TwoNumber, int> { RequestContent = new TwoNumber { One = 1, Two = 2 } });
            EventHubCore.Send<RedisPubSubChannel, TwoNumber>("testchannel", m);
        }

        public void ErrorHandlingTest()
        {
            EventHubCore.RegisterHandler<RedisPubSubChannel, TwoNumber>("ch1", MyHandler);
            Console.WriteLine("Redis DOWN, ENTER");
            Console.ReadLine();
            EventHubCore.RegisterHandler<RedisPubSubChannel, TwoNumber, int>("ch1", Add);
            Console.WriteLine("CH1 registration is success");
            try
            {
                EventHubCore.RegisterHandler<RedisPubSubChannel, TwoNumber>("ch2", MyHandler);
                Console.WriteLine("CH2 registration is success");
            }
            catch(Exception e)
            {
                Console.WriteLine($"CH2 (new channel) registration is FAIL: {e.Message}");
            }
            Console.WriteLine("Redis UP, ENTER");
            Console.ReadLine();
            Console.WriteLine($"Call withe this result: {EventHubCore.Call<RedisPubSubChannel, TwoNumber, int>("ch1", new TwoNumber() { One = 1, Two = 2 })}");
            EventHubCore.Send<RedisPubSubChannel, TwoNumber>("ch1", new TwoNumber() { One = 1, Two = 2 });
            Console.WriteLine("Send success");
            EventHubCore.SendAsync<RedisPubSubChannel, TwoNumber>("ch1", new TwoNumber() { One = 1, Two = 2 });
            Console.WriteLine("SendAsync success");
            Console.WriteLine("Redis DOWN, ENTER");
            Console.ReadLine();
            try
            {
                Console.WriteLine($"Call withe this result: {EventHubCore.Call<RedisPubSubChannel, TwoNumber, int>("ch1", new TwoNumber() { One = 1, Two = 2 })}");
            }
            catch(Exception e)
            {
                Console.WriteLine($"Call error {e.Message}");
            }
            try
            {
                EventHubCore.Send<RedisPubSubChannel, TwoNumber>("ch1", new TwoNumber() { One = 1, Two = 2 });
                Console.WriteLine("Send success");
            }
            catch(Exception e)
            {
                Console.WriteLine($"Send error {e.Message}");
            }
            try
            {
                EventHubCore.SendAsync<RedisPubSubChannel, TwoNumber>("ch1", new TwoNumber() { One = 1, Two = 2 });
                Console.WriteLine("SendAsync success");
            }
            catch(Exception e)
            {
                Console.WriteLine($"SendAsync error {e.Message}");
            }
        }

        public void Init()
        {
            EventHubCore.RegisterHandler<RedisPubSubChannel, TwoNumber>("testchannel", MyHandler);
            EventHubCore.RegisterHandler<RedisPubSubChannel, TwoNumber, int>("testchannel", Add);
            EventHubCore.RegisterHandler<RedisPubSubChannel, int>("testchannel", Print);
        }

        public void Print(Response<int> m)
        {
            Console.WriteLine("Result:");
            Console.WriteLine(m.ResponseContent);
        }

        public void MyHandler(TwoNumber m)
        {
            Console.WriteLine("Received:-------------------------------");
            Console.WriteLine(m.One);
            Console.WriteLine(m.Two);
        }

        public Response<int> Add(Request<TwoNumber, int> m)
        {
            Console.WriteLine("Received: Add -------------------------------");
            Console.WriteLine(m.RequestContent.One);
            Console.WriteLine(m.RequestContent.Two);
            var resp = m.MyResponse;
            resp.ResponseContent = m.RequestContent.One + m.RequestContent.Two;
            return resp;
        }
    }

    public class SimpleCalculatorService
    {
        public SimpleCalculatorService()
        {
            EventHubCore.RegisterHandler<RedisPubSubChannel, TwoNumber, AddResult>("calculatorservice", Add);
            EventHubCore.RegisterHandler<RedisPubSubChannel, TwoNumber, MultiplicationResult>("calculatorservice", Multiplication);
        }

        public Response<AddResult> Add(Request<TwoNumber, AddResult> r)
        {
            Program.Write($"Server side: Add ({r.RequestContent.One} + {r.RequestContent.Two})");
            var response = r.MyResponse;
            try
            {
                response.ResponseContent = new AddResult { Result = r.RequestContent.One + r.RequestContent.Two };
            }
            catch (Exception e)
            {
                response.Exception = e;
            }
            return response;
        }

        public Response<MultiplicationResult> Multiplication(Request<TwoNumber, MultiplicationResult> r)
        {
            Program.Write($"Server side: Multiplication ({r.RequestContent.One} * {r.RequestContent.Two})");
            var response = r.MyResponse;
            try
            {
                response.ResponseContent = new MultiplicationResult { Result = r.RequestContent.One * r.RequestContent.Two };
            }
            catch (Exception e)
            {
                response.Exception = e;
            }
            return response;
        }
    }

    public class CalculatorClient
    {
        public void SincTest()
        {
            CallAdd();
            CallMultiplication();
        }

        private void CallAdd()
        {
            var tn = new TwoNumber { One = 1, Two = 2 };
            Program.Write($"Client: Call Add ({tn.One} + {tn.Two})");
            var result = EventHubCore.Call<RedisPubSubChannel, TwoNumber, AddResult>("calculatorservice", tn);
            Program.Write($"Client: Recive response: {result.Result} ");
        }

        private void CallMultiplication()
        {
            var tn = new TwoNumber { One = 1, Two = 2 };
            Program.Write($"Client: Call Multiplication ({tn.One} * {tn.Two})");
            var result = EventHubCore.Call<RedisPubSubChannel, TwoNumber, MultiplicationResult>("calculatorservice", tn);
            Program.Write($"Client: Recive response: {result.Result} ");
        }

    }

    public class TwoNumber
    {
        public int One { get; set; }

        public int Two { get; set; }
    }

    public class AddResult
    {
        public int Result { get; set; }
    }

    public class MultiplicationResult
    {
        public int Result { get; set; }
    }

    public class ReferencePingPong
    {
        public ReferencePingPong(int round)
        {
            _round = round;
            Ping(new PingPongConcrat.Ping { No = 1, StartTime = DateTime.UtcNow });
        }

        public void Ping(PingPongConcrat.Ping ping)
        {
            Pong(new PingPongConcrat.Pong { No = ping.No, StartTime = ping.StartTime });
        }

        public void Pong(PingPongConcrat.Pong pong)
        {
            if (_round > pong.No)
            {
                Ping(new PingPongConcrat.Ping { No = pong.No + 1, StartTime = pong.StartTime });
            }
            else
            {
                Program.Write($"{pong.No} round in {DateTime.UtcNow.Subtract(pong.StartTime)}");
            }
        }

        public class PingPongConcrat
        {
            public class Ping
            {
                public DateTime StartTime { get; set; }

                public int No { get; set; }
            }

            public class Pong
            {
                public DateTime StartTime { get; set; }

                public int No { get; set; }
            }
        }

        private int _round = 0;
    }

    public class PingPong<TChannel>
        where TChannel : BaseChannel, new()
    {
        public PingPong(int round)
        {
            _round = round;
            EventHubCore.RegisterHandler<TChannel, PingPongConcrat.Ping>(CHANNEL_ID, Ping);
            EventHubCore.RegisterHandler<TChannel, PingPongConcrat.Pong>(CHANNEL_ID, Pong);
            EventHubCore.Send<TChannel, PingPongConcrat.Ping>(CHANNEL_ID, new PingPongConcrat.Ping { No = 1, StartTime = DateTime.UtcNow });
        }

        public void Ping(PingPongConcrat.Ping ping)
        {
            EventHubCore.Send<TChannel, PingPongConcrat.Pong>(CHANNEL_ID, new PingPongConcrat.Pong { No = ping.No, StartTime = ping.StartTime });
        }

        public void Pong(PingPongConcrat.Pong pong)
        {
            if (_round > pong.No)
            {
                EventHubCore.Send<TChannel, PingPongConcrat.Ping>(CHANNEL_ID, new PingPongConcrat.Ping { No = pong.No+1, StartTime = pong.StartTime });
            }
            else
            {
                Program.Write($"{pong.No} round in {DateTime.UtcNow.Subtract(pong.StartTime)}");
            }
        }

        const string CHANNEL_ID = "pingpong";

        private int _round = 0;

        public class PingPongConcrat
        {
            public class Ping
            {
                public DateTime StartTime { get; set; }

                public int No { get; set; }
            }

            public class Pong
            {
                public DateTime StartTime { get; set; }

                public int No { get; set; }
            }
        }
    }

    

}
