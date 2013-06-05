using System;

namespace Fleck.Wamp
{
    public interface IWampServer
    {
        void Start(Action<IWampConnection> config);
    }
}