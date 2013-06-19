using System;

namespace Fleck.Wamp.Interfaces
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
    }

}