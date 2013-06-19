using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Fleck.Wamp.Interfaces;

namespace Fleck.Wamp
{
    public class WampServer : IWampServer
    {
        private const int DefaultListeningPort = 8181;
        private const int Version = 1;
        private readonly IDictionary<Uri, ISet<IWampServerConnection>> _subscriptions;
        private readonly IWampCommsHandler _commsHandler;
        private readonly IDictionary<Guid, IWampServerConnection> _connections;

        public IDictionary<IWampServerConnection, IDictionary<string, Uri>> Prefixes { get; private set; }
        public IReadOnlyDictionary<Uri, ISet<IWampServerConnection>> Subscriptions
        {
            get { return new ReadOnlyDictionary<Uri, ISet<IWampServerConnection>>(_subscriptions); }
        }
        public IReadOnlyDictionary<Guid, IWampServerConnection> Connections
        {
            get { return new ReadOnlyDictionary<Guid, IWampServerConnection>(_connections); }
        }

        public int ProtocolVersion
        {
            get { return Version; }
        }

        public string ServerIdentity { get; private set; }

        public WampServer(string location)
            : this(DefaultListeningPort, location)
        {
        }

        public WampServer(int port, string location)
            : this(new WampCommsHandler(port, location))
        {
        }

        public WampServer(IWampCommsHandler commsHandler)
        {
            var assemblyName = Assembly.GetExecutingAssembly().GetName();
            ServerIdentity = String.Format("{0}/{1}.{2}.{3}",
                assemblyName.Name,
                assemblyName.Version.Major,
                assemblyName.Version.Minor,
                assemblyName.Version.Build);

            Prefixes = new Dictionary<IWampServerConnection, IDictionary<string, Uri>>();
            _subscriptions = new Dictionary<Uri, ISet<IWampServerConnection>>();
            _connections = new Dictionary<Guid, IWampServerConnection>();

            _commsHandler = commsHandler;            
        }

        public void Start(Action<IWampServerConnection> config)
        {
            _commsHandler.Start(socket =>
                {
                    if (socket == null)
                        throw new ArgumentNullException("socket");

                    var connection = new WampServerConnection(socket, config);

                    socket.OnCall = connection.OnCall;
                    socket.OnCallError = connection.OnCallError;
                    socket.OnCallResult = connection.OnCallResult;
                    socket.OnEvent = connection.OnEvent;

                    socket.OnClose = () => HandleOnClose(socket);
                    socket.OnOpen = () => HandleOnOpen(socket);
                    socket.OnPrefix = msg => HandleOnPrefix(socket, msg);
                    socket.OnPublish = msg => HandleOnPublish(socket, msg);
                    socket.OnSubscribe = msg => HandleOnSubscribe(socket, msg);
                    socket.OnUnsubscribe = msg => HandleOnUnsubscribe(socket, msg);
                });
        }

        public void AddSubscriptionChannel(Uri uri)
        {
            _subscriptions.Add(uri, new HashSet<IWampServerConnection>());
        }

        public void RemoveSubscriptionChannel(Uri uri)
        {
            _subscriptions.Remove(uri);
        }

        private void HandleOnUnsubscribe(IWampConnection connection, UnsubscribeMessage msg)
        {
            if (!_subscriptions.ContainsKey(msg.TopicUri))
                return;

            _subscriptions[msg.TopicUri].Remove(connection);
        }

        private void HandleOnSubscribe(IWampConnection connection, SubscribeMessage msg)
        {
            if (!_subscriptions.ContainsKey(msg.TopicUri))
                return;

            _subscriptions[msg.TopicUri].Add(connection);
        }

        private void HandleOnPublish(IWampConnection connection, PublishMessage msg)
        {
            if (!_subscriptions.ContainsKey(msg.TopicUri))
                return;

            var subscriptions = new HashSet<IWampServerConnection>(msg.Eligible == null
                    ? _subscriptions[msg.TopicUri]
                    : _subscriptions[msg.TopicUri].Where(x => msg.Eligible.Contains(x.WebSocketConnectionInfo.Id)));

            if (msg.ExcludeMe.HasValue)
                subscriptions.RemoveWhere(x => x.WebSocketConnectionInfo.Id == connection.WebSocketConnectionInfo.Id);

            if (msg.Exclude != null)
                subscriptions.RemoveWhere(x => msg.Exclude.Contains(x.WebSocketConnectionInfo.Id));

            foreach (var subscription in subscriptions)
                subscription.SendPublish(msg);
        }

        private void HandleOnPrefix(IWampConnection connection, PrefixMessage msg)
        {
            if (!Prefixes.ContainsKey(connection))
                Prefixes.Add(connection, new Dictionary<string, Uri>());

            var prefixes = Prefixes[connection];

            prefixes[msg.Prefix] = msg.Uri;
        }

        private void HandleOnOpen(IWampConnection connection)
        {
            // Add connection to lookup
            var connId = connection.WebSocketConnectionInfo.Id;
            _connections.Add(connId, connection);

            var message = new WelcomeMessage()
            {
                SessionId = connection.WebSocketConnectionInfo.Id,
                ProtocolVersion = ProtocolVersion,
                ServerIdentity = ServerIdentity
            };
            connection.SendWelcome(message);
        }

        private void HandleOnClose(IWampConnection connection)
        {
            // Remove connection from lookup
            _connections.Remove(connection.WebSocketConnectionInfo.Id);

            // Remove prefixes
            Prefixes.Remove(connection);

            // Remove subscriptions
            foreach (var topics in Subscriptions)
            {
                topics.Value.Remove(connection);
            }
        }
    
        public void SendEvent(EventMessage msg, ISet<Guid> includes = null, ISet<Guid> excludes = null)
        {
            if (!_subscriptions.ContainsKey(msg.TopicUri))
                return;

            var s = includes == null
                ? _subscriptions[msg.TopicUri]
                : _subscriptions[msg.TopicUri].Where(t => includes.Contains(t.WebSocketConnectionInfo.Id));

            var s2 = excludes == null
                ? s
                : s.Where(t => !excludes.Contains(t.WebSocketConnectionInfo.Id));

            foreach (var subscription in s2)
                subscription.SendEvent(msg);
        }

        public void AddSubscription(Uri uri, IWampServerConnection connection)
        {
            if (!_subscriptions.ContainsKey(uri))
                return;

            _subscriptions[uri].Add(connection);
        }

        public void RemoveSubscription(Uri uri, IWampServerConnection connection)
        {
            if (!_subscriptions.ContainsKey(uri))
                return;

            _subscriptions[uri].Remove(connection);
        }

        public void Dispose()
        {
        }
    }
}
