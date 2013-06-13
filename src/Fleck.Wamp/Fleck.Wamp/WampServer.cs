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

        public void AddSubcriptionChannel(Uri uri)
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

            var subscriptions = msg.Eligible == null
                    ? _subscriptions[msg.TopicUri]
                    : _subscriptions[msg.TopicUri].Where(x =>
                        {
                            var c = connection;
                            return msg.Eligible.Contains(c.WebSocketConnectionInfo.Id);
                        })
                .Where(x =>
                    {
                        var c = connection;
                        if (msg.ExcludeMe.HasValue && 
                            msg.ExcludeMe.Value && 
                            x.WebSocketConnectionInfo.Id == c.WebSocketConnectionInfo.Id)
                            return false;

                        return !msg.Exclude.Contains(x.WebSocketConnectionInfo.Id);
                    });

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
    
    }
}
