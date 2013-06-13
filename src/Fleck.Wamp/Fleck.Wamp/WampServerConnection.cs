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
            OnPublish = message => { };
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
        public Action<PublishMessage> OnPublish { get; set; }

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

        public void SendPublish(PublishMessage message)
        {
            _wampConnection.SendPublish(message);
        }

        #region Equality implementation
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((WampServerConnection)obj);
        }

        protected bool Equals(WampServerConnection other)
        {
            return Equals(_wampConnection, other._wampConnection);
        }

        public override int GetHashCode()
        {
            return (_wampConnection != null ? _wampConnection.GetHashCode() : 0);
        }

        public static bool operator ==(WampServerConnection left, WampServerConnection right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(WampServerConnection left, WampServerConnection right)
        {
            return !Equals(left, right);
        }
        #endregion
    }
}