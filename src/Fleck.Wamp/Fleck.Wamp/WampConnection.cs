using System;
using Newtonsoft.Json;

namespace Fleck.Wamp
{
    public class WampConnection : IWampConnection
    {
        private readonly IWebSocketConnection _webSocketConnection;

        public WampConnection(IWebSocketConnection webSocketConnection, Action<IWampConnection> initialize)
        {
            OnOpen = () => { };
            OnClose = () => { };
            OnWelcome = message => { };
            OnPrefix = message => { };
            OnCall = message => { };
            OnSubscribe = message => { };
            OnUnsubscribe = message => { };
            OnPublish = message => { };
            initialize(this);

            _webSocketConnection = webSocketConnection;
            _webSocketConnection.OnOpen = () => OnOpen();
            _webSocketConnection.OnClose = () => OnClose();
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

        public Action OnOpen { get; set; }
        public Action OnClose { get; set; }
        public Action<WelcomeMessage> OnWelcome { get; set; }
        public Action<PrefixMessage> OnPrefix{ get; set; }
        public Action<CallMessage> OnCall { get; set; }
        public Action<CallResultMessage> OnCallResult { get; set; }
        public Action<CallErrorMessage> OnCallError { get; set; }
        public Action<SubscribeMessage> OnSubscribe { get; set; }
        public Action<UnsubscribeMessage> OnUnsubscribe { get; set; }
        public Action<PublishMessage> OnPublish { get; set; }
        public Action<EventMessage> OnEvent { get; set; }

        public void SendWelcome(WelcomeMessage message)
        {
            SendMessage(message);
            OnWelcome(message);
        }

        public void SendCallResult(CallResultMessage message)
        {
            SendMessage(message);
            OnCallResult(message);
        }

        public void SendCallError(CallErrorMessage message)
        {
            SendMessage(message);
            OnCallError(message);
        }

        public void SendEvent(EventMessage message)
        {
            SendMessage(message);
            OnEvent(message);
        }

        private void SendMessage(IWampMessage message)
        {
            var msg = JsonConvert.SerializeObject(message);
            Send(msg);
        }
    }
}
