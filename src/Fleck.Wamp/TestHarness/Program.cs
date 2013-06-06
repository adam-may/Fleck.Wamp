using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fleck;
using Fleck.Wamp;
using Newtonsoft.Json;

namespace TestHarness
{
    class Program
    {
        static void Main(string[] args)
        {
            var converter = new WampJsonConverter();
            var msg = new WelcomeMessage() {ProtocolVersion = 1, ServerIdentity = "Test/1.0.0", SessionId = "v59mbCGDXZ7WTyxB"};
            var result = JsonConvert.SerializeObject(msg);
            var test = JsonConvert.DeserializeObject<WampMessage>(result);

            FleckLog.Level = LogLevel.Debug;
            var allSockets = new List<IWebSocketConnection>();
            var server = new WampServer("ws://localhost:8181");
            server.Start(wamp =>
                {
                    
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
    }
}
