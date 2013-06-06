using System;
using System.Reflection;
using Newtonsoft.Json;

namespace Fleck.Wamp
{
    public class WampCommsHandler : IWampCommsHandler
    {
        private readonly WebSocketServer _webSocketServer;
        private const int DefaultListeningPort = 8181;
        private const string WampSubProtocol = "wamp";
        private const int ProtocolVersionConst = 1;

        public int ProtocolVersion
        {
            get { return ProtocolVersionConst; }
        }
        public string ServerIdentity { get; set; }

        public WampCommsHandler(int port, string location)
        {
            var assemblyName = Assembly.GetExecutingAssembly().GetName();
            ServerIdentity = String.Format("{0}/{1}.{2}.{3}",
                assemblyName.Name,
                assemblyName.Version.Major,
                assemblyName.Version.Minor,
                assemblyName.Version.Build);

            _webSocketServer = new WebSocketServer(port, location);
        }

        public WampCommsHandler(string location)
            : this(DefaultListeningPort, location)
        {
        }

        public void Start(Action<IWampConnection> config)
        {
            _webSocketServer.Start(socket =>
            {
                if (socket == null)
                    throw new ArgumentNullException("socket");
                if (socket.ConnectionInfo == null) 
                    throw new ArgumentNullException("ConnectionInfo");
                if (!socket.ConnectionInfo.SubProtocol.Equals(WampSubProtocol))
                    throw new ArgumentException("SubProtocol");

                var connection = new WampConnection(socket, config);

                socket.OnOpen = () => HandleOnOpen(connection);
                socket.OnClose = connection.OnClose;
                socket.OnMessage = msg => OnMessage(connection, msg);
            });            
        }

        private static void OnMessage(IWampConnection connection, string msg)
        {
            var message = JsonConvert.DeserializeObject<IWampMessage>(msg);

            switch (message.MessageType)
            {
                case MessageType.Prefix:
                    connection.OnPrefix(message as PrefixMessage);
                    break;
                case MessageType.Call:
                    connection.OnCall(message as CallMessage);
                    break;
                case MessageType.Subscribe:
                    connection.OnSubscribe(message as SubscribeMessage);
                    break;
                case MessageType.Unsubscribe:
                    connection.OnUnsubscribe(message as UnsubscribeMessage);
                    break;
                case MessageType.Publish:
                    connection.OnPublish(message as PublishMessage);
                    break;
                default:
                    throw new ArgumentException("msg");
            }
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
