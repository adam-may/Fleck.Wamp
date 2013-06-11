using System;

namespace Fleck.Wamp.Interfaces
{
    public interface IWampServer
    {
        int ProtocolVersion { get; }
        string ServerIdentity { get; }
        void Start(Action<IWampServerConnection> config);
    }
}