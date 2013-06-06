using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fleck.Wamp
{
    public interface IWampConnection
    {
        IWebSocketConnectionInfo WebSocketConnectionInfo { get; }
        Action OnOpen { get; }
        Action OnClose { get; }
        void Close();
        Action<WelcomeMessage> OnWelcome { get; set; }
        Action<PrefixMessage> OnPrefix { get; set; }
        Action<CallMessage> OnCall { get; set; }
        Action<CallResultMessage> OnCallResult { get; set; }
        Action<CallErrorMessage> OnCallError { get; set; }
        Action<SubscribeMessage> OnSubscribe { get; set; }
        Action<UnsubscribeMessage> OnUnsubscribe { get; set; }
        Action<PublishMessage> OnPublish { get; set; }
        Action<EventMessage> OnEvent { get; set; }
        void SendWelcome(WelcomeMessage message);
        void SendCallResult(CallResultMessage message);
        void SendCallError(CallErrorMessage message);
        void SendEvent(EventMessage message);
    }

}