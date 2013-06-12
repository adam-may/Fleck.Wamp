using System;
using System.Collections.Generic;

namespace Fleck.Wamp.Interfaces
{
    public interface IWampServer
    {
        int ProtocolVersion { get; }
        string ServerIdentity { get; }
        IDictionary<Guid, IDictionary<string, Uri>> Prefixes { get; }
        IReadOnlyDictionary<Uri, ISet<Guid>> Subscriptions { get; }

        void Start(Action<IWampServerConnection> config);
        void AddSubcriptionChannel(Uri uri);
    }
}