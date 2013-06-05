using System;

namespace Fleck.Wamp
{
    public class WampServer : IWampServer
    {
        private readonly WebSocketServer _webSocketServer;
        private const int DefaultListeningPort = 8181;
        private const string WampSubProtocol = "wamp";

        public WampServer(int port, string location)
        {
            _webSocketServer = new WebSocketServer(port, location);
        }

        public WampServer(string location)
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

                socket.OnOpen = () => OnOpen(socket);
                socket.OnClose = () => OnClose(socket);
                socket.OnMessage = msg => OnMessage(socket, msg);
            });            
        }

        private static void OnOpen(IWebSocketConnection connection)
        {
        }

        private static void OnClose(IWebSocketConnection connection)
        {
        }

        private static void OnMessage(IWebSocketConnection connection, string msg)
        {
        }
    }
}
