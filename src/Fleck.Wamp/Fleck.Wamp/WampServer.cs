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
        private const int ProtocolVersionConst = 1;
        private readonly IDictionary<Uri, ISet<Guid>> _subscriptions;
        private readonly IWampCommsHandler _commsHandler;

        public IDictionary<Guid, IDictionary<string, Uri>> Prefixes { get; private set; }
        public IReadOnlyDictionary<Uri, ISet<Guid>> Subscriptions
        {
            get { return new ReadOnlyDictionary<Uri, ISet<Guid>>(_subscriptions); }
        }

        public int ProtocolVersion
        {
            get { return ProtocolVersionConst; }
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

            Prefixes = new Dictionary<Guid, IDictionary<string, Uri>>();
            _subscriptions = new Dictionary<Uri, ISet<Guid>>();

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
            _subscriptions.Add(uri, new HashSet<Guid>());
        }

        private void HandleOnUnsubscribe(IWampConnection connection, UnsubscribeMessage msg)
        {
            var connId = connection.WebSocketConnectionInfo.Id;

            if (!Subscriptions.ContainsKey(msg.TopicUri))
                return;

            var subscriptions = Subscriptions[msg.TopicUri];

            subscriptions.Remove(connId);
        }

        private void HandleOnSubscribe(IWampConnection connection, SubscribeMessage msg)
        {
            var connId = connection.WebSocketConnectionInfo.Id;

            if (!Subscriptions.ContainsKey(msg.TopicUri))
                return;

            var subscriptions = Subscriptions[msg.TopicUri];

            subscriptions.Add(connId);
        }

        private void HandleOnPublish(IWampConnection connection, PublishMessage msg)
        {
        }

        private void HandleOnPrefix(IWampConnection connection, PrefixMessage msg)
        {
            var connId = connection.WebSocketConnectionInfo.Id;
            
            if (!Prefixes.ContainsKey(connId))
                Prefixes.Add(connId, new Dictionary<string, Uri>());

            var prefixes = Prefixes[connId];

            prefixes[msg.Prefix] = msg.Uri;
        }

        private void HandleOnOpen(IWampConnection connection)
        {
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
            var connId = connection.WebSocketConnectionInfo.Id;

            Prefixes.Remove(connId);

            foreach (var topics in Subscriptions)
            {
                topics.Value.Remove(connId);
            }
        }
    
    }
}
