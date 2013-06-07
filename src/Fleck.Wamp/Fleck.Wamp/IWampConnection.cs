using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fleck.Wamp
{
    public interface IWampConnection : IWampServerConnection
    {
        Action OnOpen { get; set; }
        Action OnClose { get; set; }
        void Close();
        void SendWelcome(WelcomeMessage message);
        Action<WelcomeMessage> OnWelcome { get; set; }
        Action<PrefixMessage> OnPrefix { get; set; }
        Action<SubscribeMessage> OnSubscribe { get; set; }
        Action<UnsubscribeMessage> OnUnsubscribe { get; set; }
        Action<PublishMessage> OnPublish { get; set; }
    }

}