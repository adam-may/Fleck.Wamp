using System;

namespace Fleck.Wamp
{
    public class WampConnection : IWampConnection
    {
        private readonly IWebSocketConnection _webSocketConnection;

        public WampConnection(IWebSocketConnection webSocketConnection)
        {
            _webSocketConnection = webSocketConnection;
            OnWelcome = () => { };
            OnPrefix = message => { };
            OnCall = message => { };
            OnSubscribe = message => { };
            OnUnsubscribe = message => { };
            OnPublish = message => { };
        }

        public IWebSocketConnectionInfo WebSocketConnectionInfo
        {
            get { return _webSocketConnection.ConnectionInfo; }
        }

        public void Send(string message)
        {
            _webSocketConnection.Send(message);
        }

        public void Send(byte[] message)
        {
            _webSocketConnection.Send(message);
        }

        public void Close()
        {
            _webSocketConnection.Close();
        }


        public Action OnWelcome { get; set; }
        public Action<PrefixMessage> OnPrefix{ get; set; }
        public Action<CallMessage> OnCall { get; set; }
        public void SendCallResult(CallResultMessage message)
        {
            throw new NotImplementedException();
        }
        public void SendCallError(CallErrorMessage message)
        {
            throw new NotImplementedException();
        }
        public Action<SubscribeMessage> OnSubscribe { get; set; }
        public Action<UnsubscribeMessage> OnUnsubscribe { get; set; }
        public Action<PublishMessage> OnPublish { get; set; }
        public void SendEvent(EventMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
