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
        void Close();
        Action OnWelcome { get; set; }
        Action<PrefixMessage> OnPrefix { get; set; }
        Action<CallMessage> OnCall { get; set; }
        void SendCallResult(CallResultMessage message);
        void SendCallError(CallErrorMessage message);
        Action<SubscribeMessage> OnSubscribe { get; set; }
        Action<UnsubscribeMessage> OnUnsubscribe { get; set; }
        Action<PublishMessage> OnPublish { get; set; }
        void SendEvent(EventMessage message);
    }

}