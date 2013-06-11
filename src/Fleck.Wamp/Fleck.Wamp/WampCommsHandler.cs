using System;
using System.Reflection;
using Newtonsoft.Json;

namespace Fleck.Wamp
{
    public class WampCommsHandler : IWampCommsHandler
    {
        private readonly IWebSocketServer _webSocketServer;
        private const string WampSubProtocol = "wamp";

        public WampCommsHandler(string location)
        {
            _webSocketServer = new WebSocketServer(location);
        }

        public WampCommsHandler(int port, string location)
        {
            _webSocketServer = new WebSocketServer(port, location);
        }

        public WampCommsHandler(IWebSocketServer webSocketServer)
        {
            _webSocketServer = webSocketServer;
        }

        public string ServerIdentity { get; set; }

        public void Start(Action<IWampConnection> config)
        {
            _webSocketServer.Start(socket =>
            {
                if (socket == null)
                    throw new ArgumentNullException("socket");
                if (socket.ConnectionInfo == null) 
                    throw new ArgumentNullException("socket.ConnectionInfo");
                if (!socket.ConnectionInfo.SubProtocol.Equals(WampSubProtocol))
                    throw new ArgumentException("SubProtocol");

                var connection = new WampConnection(socket, config);

                socket.OnOpen = connection.OnOpen;
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
    }
}
