using System;
using System.Collections.Generic;

namespace Fleck.Wamp.Interfaces
{
    public interface IWampServer : IDisposable
    {
        int ProtocolVersion { get; }
        string ServerIdentity { get; }
        IDictionary<IWampServerConnection, IDictionary<string, Uri>> Prefixes { get; }
        IReadOnlyDictionary<Uri, ISet<IWampServerConnection>> Subscriptions { get; }
        IReadOnlyDictionary<Guid, IWampServerConnection> Connections { get; }

        void Start(Action<IWampServerConnection> config);
        void AddSubscriptionChannel(Uri uri);
        void RemoveSubscriptionChannel(Uri uri);
        void AddSubscription(Uri uri, IWampServerConnection connection);
        void RemoveSubscription(Uri uri, IWampServerConnection connection);

        void SendEvent(EventMessage msg, ISet<Guid> includes = null, ISet<Guid> excludes = null);
    }
}