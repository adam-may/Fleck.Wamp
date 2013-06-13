using System;

namespace Fleck.Wamp.Interfaces
{
    public interface IWampServerConnection
    {
        IWebSocketConnectionInfo WebSocketConnectionInfo { get; }
        Action<CallMessage> OnCall { get; set; }
        Action<CallResultMessage> OnCallResult { get; set; }
        Action<CallErrorMessage> OnCallError { get; set; }
        Action<EventMessage> OnEvent { get; set; }
        void SendCallResult(CallResultMessage message);
        void SendCallError(CallErrorMessage message);
        void SendEvent(EventMessage message);
        void SendPublish(PublishMessage message);
    }
}