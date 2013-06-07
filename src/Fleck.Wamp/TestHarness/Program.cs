using System;
using System.Collections.Generic;
using System.Linq;
using Fleck;
using Fleck.Wamp;

namespace TestHarness
{
    class Program
    {
        static void Main()
        {
            FleckLog.Level = LogLevel.Debug;
            var allSockets = new List<IWampConnection>();
            var server = new WampServer("ws://localhost:8181");
            server.Start(wamp =>
                {
                    wamp.OnEvent = OnEvent;
                    wamp.OnCall = OnCall;
                });

            var input = Console.ReadLine();
            while (input != "exit")
            {
                input = Console.ReadLine();
            }
        }

        private static void OnCall(CallMessage callMessage)
        {
            Console.WriteLine("Event message received: {0}", callMessage.CallId);
        }

        private static void OnEvent(EventMessage eventMessage)
        {
            Console.WriteLine("Event message received: {0}", eventMessage.Event);
        }
    }
}
