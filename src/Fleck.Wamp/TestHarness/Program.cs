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
            var allSockets = new List<IWebSocketConnection>();
            var server = new WampCommsHandler("ws://localhost:8181");
            server.Start(wamp =>
                {
                    wamp.OnWelcome = msg => OnWelcome(wamp, msg);
                    wamp.OnPrefix = msg => OnPrefix(wamp, msg);
                });

            var input = Console.ReadLine();
            while (input != "exit")
            {
                foreach (var socket in allSockets.ToList())
                {
                    socket.Send(input);
                }
                input = Console.ReadLine();
            }
        }

        private static void OnPrefix(IWampConnection wampConnection, PrefixMessage msg)
        {
            throw new NotImplementedException();
        }

        private static void OnWelcome(IWampConnection connection, WelcomeMessage msg)
        {
            Console.WriteLine("Welcome message received");
        }
    }
}
