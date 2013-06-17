using System;

namespace Fleck.Wamp.Interfaces
{
    public interface IWampCommsHandler
    {
        void Start(Action<IWampConnection> config);
    }
}