using System;
using System.Collections.Generic;

namespace Fleck.Wamp.Interfaces
{
    public interface IWampServer
    {
        int ProtocolVersion { get; }
        string ServerIdentity { get; }
        IDictionary<IWampServerConnection, IDictionary<string, Uri>> Prefixes { get; }
        IReadOnlyDictionary<Uri, ISet<IWampServerConnection>> Subscriptions { get; }

        void Start(Action<IWampServerConnection> config);
        void AddSubcriptionChannel(Uri uri);
    }
}