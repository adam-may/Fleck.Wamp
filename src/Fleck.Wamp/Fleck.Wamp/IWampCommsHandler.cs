using System;

namespace Fleck.Wamp
{
    public interface IWampCommsHandler
    {
        void Start(Action<IWampConnection> config);
    }
}