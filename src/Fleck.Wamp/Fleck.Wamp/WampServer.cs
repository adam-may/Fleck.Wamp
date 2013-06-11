using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Fleck.Wamp.Interfaces;

namespace Fleck.Wamp
{
    public class WampServer : IWampServer
    {
        private const int DefaultListeningPort = 8181;
        private readonly IWampCommsHandler _commsHandler;

        public int ProtocolVersion { get; private set; }
        public string ServerIdentity { get; private set; }

        public WampServer(string location)
            : this(DefaultListeningPort, location)
        {
        }

        public WampServer(int port, string location)
            : this(new WampCommsHandler(port, location))
        {
        }

        public WampServer(IWampCommsHandler commsHandler)
        {
            var assemblyName = Assembly.GetExecutingAssembly().GetName();
            ServerIdentity = String.Format("{0}/{1}.{2}.{3}",
                assemblyName.Name,
                assemblyName.Version.Major,
                assemblyName.Version.Minor,
                assemblyName.Version.Build);

            ProtocolVersion = 1;

            _commsHandler = commsHandler;            
        }

        public void Start(Action<IWampServerConnection> config)
        {
            _commsHandler.Start(socket =>
                {
                    if (socket == null)
                        throw new ArgumentNullException("socket");

                    var connection = new WampServerConnection(socket, config);

                    socket.OnCall = connection.OnCall;
                    socket.OnCallError = connection.OnCallError;
                    socket.OnCallResult = connection.OnCallResult;
                    socket.OnEvent = connection.OnEvent;

                    socket.OnClose = () => HandleOnClose(socket);
                    socket.OnOpen = () => HandleOnOpen(socket);
                    socket.OnPrefix = msg => HandleOnPrefix(socket, msg);
                    socket.OnPublish = msg => HandleOnPublish(socket, msg);
                    socket.OnSubscribe = msg => HandleOnSubscribe(socket, msg);
                    socket.OnUnsubscribe = msg => HandleOnUnsubscribe(socket, msg);
                    socket.OnWelcome = msg => HandleOnWelcome(socket, msg);
                });
        }

        private void HandleOnWelcome(IWampConnection socket, WelcomeMessage msg)
        {
        }

        private void HandleOnUnsubscribe(IWampConnection socket, UnsubscribeMessage msg)
        {
        }

        private void HandleOnSubscribe(IWampConnection socket, SubscribeMessage subscribeMessage)
        {
        }

        private void HandleOnPublish(IWampConnection connection, PublishMessage publishMessage)
        {
        }

        private void HandleOnPrefix(IWampConnection connection, PrefixMessage prefixMessage)
        {
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

        private void HandleOnClose(IWampConnection connection)
        {
        }
    
    }
}
