using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Fleck.Wamp.Interfaces;

namespace Fleck.Wamp
{
    public class WampServer
    {
        private const int DefaultListeningPort = 8181;
        private readonly IWampCommsHandler _commsHandler;

        public int ProtocolVersion { get; private set; }
        public string ServerIdentity { get; set; }

        public WampServer(string location)
            :this(DefaultListeningPort, location)
        {
        }

        public WampServer(int port, string location)
        {
            var assemblyName = Assembly.GetExecutingAssembly().GetName();
            ServerIdentity = String.Format("{0}/{1}.{2}.{3}",
                assemblyName.Name,
                assemblyName.Version.Major,
                assemblyName.Version.Minor,
                assemblyName.Version.Build);

            ProtocolVersion = 1;

            _commsHandler = new WampCommsHandler(port, location);
        }

        public void Start(Action<IWampServerConnection> config)
        {
            _commsHandler.Start(socket =>
                {
                    var connection = new WampServerConnection(socket)
                        {
                            OnCall = socket.OnCall,
                            OnCallError = socket.OnCallError,
                            OnCallResult = socket.OnCallResult,
                            OnEvent = socket.OnEvent
                        };

                    socket.OnOpen = () => HandleOnOpen(socket);
                    socket.OnPrefix = msg => OnPrefix(socket, msg);
                });
        }

        private void OnPrefix(IWampConnection connection, PrefixMessage prefixMessage)
        {
            throw new NotImplementedException();
        }

        private void HandleOnOpen(IWampConnection connection)
        {
            var message = new WelcomeMessage()
            {
                SessionId = connection.WebSocketConnectionInfo.Id,
                ProtocolVersion = ProtocolVersion,
                ServerIdentity = ServerIdentity
            };
            connection.SendWelcome(message);
        }
    }
}
