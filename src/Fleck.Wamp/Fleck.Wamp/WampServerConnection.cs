using System;
using Fleck.Wamp.Interfaces;

namespace Fleck.Wamp
{
    public class WampServerConnection : IWampServerConnection
    {
        private readonly IWampConnection _wampConnection;

        public WampServerConnection(IWampConnection wampConnection, Action<IWampServerConnection> initialize)
        {
            OnCall = message => { };
            OnCallResult = message => { };
            OnCallError = message => { };
            OnEvent = message => { };
            initialize(this);

            _wampConnection = wampConnection;
        }

        public IWebSocketConnectionInfo WebSocketConnectionInfo
        {
            get { return _wampConnection.WebSocketConnectionInfo; }
        }

        public Action<CallMessage> OnCall { get; set; }
        public Action<CallResultMessage> OnCallResult { get; set; }
        public Action<CallErrorMessage> OnCallError { get; set; }
        public Action<EventMessage> OnEvent { get; set; }

        public void SendCallResult(CallResultMessage message)
        {
            _wampConnection.SendCallResult(message);
        }

        public void SendCallError(CallErrorMessage message)
        {
            _wampConnection.SendCallError(message);
        }

        public void SendEvent(EventMessage message)
        {
            _wampConnection.SendEvent(message);
        }
    }
}