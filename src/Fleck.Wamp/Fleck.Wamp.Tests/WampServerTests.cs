using System;
using System.Collections.Generic;
using System.Linq;
using Fleck.Wamp.Interfaces;
using Moq;
using NUnit.Framework;

namespace Fleck.Wamp.Tests
{
    [TestFixture]
    public class WampServerTests
    {
        private IWampServer _wampServer;
        private Mock<IWampCommsHandler> _commsMock;
        private List<Mock<IWampConnection>> _connections;
        private const int ProtocolVersion = 1;
        private const string ServerIdentity = "Fleck.Wamp/0.9.6";

        private Mock<IWampConnection> CreateMockConnection()
        {
            var connGuid = Guid.NewGuid();

            var connInfoMock = new Mock<IWebSocketConnectionInfo>();
            var connMock = new Mock<IWampConnection>();

            connInfoMock.SetupGet(x => x.SubProtocol).Returns("wamp");
            connInfoMock.SetupGet(x => x.Id).Returns(connGuid);

            connMock.SetupGet(x => x.WebSocketConnectionInfo).Returns(connInfoMock.Object);
            connMock.SetupAllProperties();

            return connMock;
        }

        [SetUp]
        public void Setup()
        {
            _connections = new List<Mock<IWampConnection>>();
            _commsMock = new Mock<IWampCommsHandler>();

            _commsMock.Setup(x => x.Start(It.IsAny<Action<IWampConnection>>()))
                      .Callback<Action<IWampConnection>>(x =>
                      {
                          var c = CreateMockConnection();
                          _connections.Add(c);
                          x(c.Object);
                      });

            _wampServer = new WampServer(_commsMock.Object);
        }

        [Test]
        public void TestProtocolVersionIsCorrect()
        {
            Assert.AreEqual(ProtocolVersion, _wampServer.ProtocolVersion);
        }

        [Test]
        public void TestServerIdentityIsCorrect()
        {
            Assert.AreEqual(ServerIdentity, _wampServer.ServerIdentity);
        }

        [Test]
        public void TestServerSendsWelcomeOnOpen()
        {
            _wampServer.Start(config => { });

            WelcomeMessage welcomeMsg = null;

            var connMock = _connections.First();

            connMock.Setup(x => x.SendWelcome(It.IsAny<WelcomeMessage>()))
                     .Callback<WelcomeMessage>(x => welcomeMsg = x);

            connMock.Object.OnOpen();

            Assert.IsNotNull(welcomeMsg);
            Assert.IsInstanceOf<WelcomeMessage>(welcomeMsg);
            Assert.AreEqual(MessageType.Welcome, welcomeMsg.MessageType);
            Assert.AreEqual(1, welcomeMsg.ProtocolVersion);
            Assert.AreEqual(connMock.Object.WebSocketConnectionInfo.Id, welcomeMsg.SessionId);
            Assert.AreEqual(ServerIdentity, welcomeMsg.ServerIdentity);
        }

        [Test]
        public void TestAddRemovePrefix()
        {
            const string prefix = "calc";
            var uri = new Uri("http://example.com/simple/calc#");

            var msg = new PrefixMessage {Prefix = prefix, Uri = uri};

            _wampServer.Start(config => { });

            var connMock = _connections.First();

            connMock.Object.OnPrefix(msg);

            Assert.IsTrue(_wampServer.Prefixes.ContainsKey(connMock.Object));
            Assert.IsTrue(_wampServer.Prefixes[connMock.Object].ContainsKey(prefix));
            Assert.AreEqual(uri, _wampServer.Prefixes[connMock.Object][prefix]);

            connMock.Object.OnClose();

            Assert.IsFalse(_wampServer.Prefixes.ContainsKey(connMock.Object));
        }

        [TestCase("http://example.com/simple")]
        [TestCase("event:myevent1")]
        [Test]
        public void TestAddRemoveSubscription(string uriString)
        {
            var uri = new Uri(uriString);
            
            var msg = new SubscribeMessage {TopicUri = uri};

            _wampServer.AddSubcriptionChannel(uri);
            _wampServer.Start(config => { });

            var connMock = _connections.First();

            connMock.Object.OnSubscribe(msg);

            Assert.IsTrue(_wampServer.Subscriptions.ContainsKey(uri));
            Assert.IsTrue(_wampServer.Subscriptions[uri].Contains(connMock.Object));
        }

        [TestCase("http://example.com/simple")]
        [Test]
        public void TestPublishWithNoSubscriptions(string uriString)
        {
            var firstCalled = false;
            var secondCalled = false;
            var thirdCalled = false;

            var uri = new Uri(uriString);

            _wampServer.Start(config =>
            {
                config.OnPublish = msg => firstCalled = true;
            });
            _wampServer.Start(config =>
            {
                config.OnPublish = msg => secondCalled = true;
            });
            _wampServer.Start(config =>
            {
                config.OnPublish = msg => thirdCalled = true;
            });

            var connMock1 = _connections.First();

            var m = new PublishMessage
                {
                    TopicUri = uri
                };

            connMock1.Object.OnPublish(m);

            Assert.IsFalse(firstCalled);
            Assert.IsFalse(secondCalled);
            Assert.IsFalse(thirdCalled);
        }

        [TestCase("http://example.com/simple")]
        [Test]
        public void TestPublishToAll(string uriString)
        {
            var firstCalled = false;
            var secondCalled = false;
            var thirdCalled = false;

            var uri = new Uri(uriString);

            _wampServer.AddSubcriptionChannel(uri);

            _wampServer.Start(config => { });
            _wampServer.Start(config => { });
            _wampServer.Start(config => { });

            var connMock1 = _connections.First();
            var connMock2 = _connections.Skip(1).First();
            var connMock3 = _connections.Skip(2).First();

            var subscribeMsg = new SubscribeMessage { TopicUri = uri };

            connMock1.Object.OnSubscribe(subscribeMsg);
            connMock2.Object.OnSubscribe(subscribeMsg);
            connMock3.Object.OnSubscribe(subscribeMsg);

            connMock1.Setup(x => x.SendPublish(It.IsAny<PublishMessage>())).Callback(() => firstCalled = true);
            connMock2.Setup(x => x.SendPublish(It.IsAny<PublishMessage>())).Callback(() => secondCalled = true);
            connMock3.Setup(x => x.SendPublish(It.IsAny<PublishMessage>())).Callback(() => thirdCalled = true);

            var m = new PublishMessage
            {
                TopicUri = uri
            };

            connMock1.Object.OnPublish(m);

            Assert.IsTrue(firstCalled);
            Assert.IsTrue(secondCalled);
            Assert.IsTrue(thirdCalled);
        }

        [TestCase("http://example.com/simple")]
        [Test]
        public void TestPublishToSome(string uriString)
        {
            var firstCalled = false;
            var secondCalled = false;
            var thirdCalled = false;

            var uri = new Uri(uriString);

            _wampServer.AddSubcriptionChannel(uri);

            _wampServer.Start(config => { });
            _wampServer.Start(config => { });
            _wampServer.Start(config => { });

            var connMock1 = _connections.First();
            var connMock2 = _connections.Skip(1).First();
            var connMock3 = _connections.Skip(2).First();

            var subscribeMsg = new SubscribeMessage { TopicUri = uri };

            // Missing second connection from the list of subscribers
            connMock1.Object.OnSubscribe(subscribeMsg);
            connMock3.Object.OnSubscribe(subscribeMsg);

            connMock1.Setup(x => x.SendPublish(It.IsAny<PublishMessage>())).Callback(() => firstCalled = true);
            connMock2.Setup(x => x.SendPublish(It.IsAny<PublishMessage>())).Callback(() => secondCalled = true);
            connMock3.Setup(x => x.SendPublish(It.IsAny<PublishMessage>())).Callback(() => thirdCalled = true);

            var m = new PublishMessage
            {
                TopicUri = uri
            };

            connMock1.Object.OnPublish(m);

            Assert.IsTrue(firstCalled);
            Assert.IsFalse(secondCalled);
            Assert.IsTrue(thirdCalled);
        }

        [TestCase("http://example.com/simple")]
        [Test]
        public void TestPublishExcludingSender(string uriString)
        {
            var firstCalled = false;
            var secondCalled = false;
            var thirdCalled = false;

            var uri = new Uri(uriString);

            _wampServer.AddSubcriptionChannel(uri);

            _wampServer.Start(config => { });
            _wampServer.Start(config => { });
            _wampServer.Start(config => { });

            var connMock1 = _connections.First();
            var connMock2 = _connections.Skip(1).First();
            var connMock3 = _connections.Skip(2).First();

            var subscribeMsg = new SubscribeMessage { TopicUri = uri };

            connMock1.Object.OnSubscribe(subscribeMsg);
            connMock2.Object.OnSubscribe(subscribeMsg);
            connMock3.Object.OnSubscribe(subscribeMsg);

            connMock1.Setup(x => x.SendPublish(It.IsAny<PublishMessage>())).Callback(() => firstCalled = true);
            connMock2.Setup(x => x.SendPublish(It.IsAny<PublishMessage>())).Callback(() => secondCalled = true);
            connMock3.Setup(x => x.SendPublish(It.IsAny<PublishMessage>())).Callback(() => thirdCalled = true);

            var m = new PublishMessage
            {
                TopicUri = uri,
                ExcludeMe = true
            };

            connMock1.Object.OnPublish(m);

            Assert.IsFalse(firstCalled);
            Assert.IsTrue(secondCalled);
            Assert.IsTrue(thirdCalled);
        }

        [TestCase("http://example.com/simple")]
        [Test]
        public void TestPublishEligibleList(string uriString)
        {
            var firstCalled = false;
            var secondCalled = false;
            var thirdCalled = false;

            var uri = new Uri(uriString);

            _wampServer.AddSubcriptionChannel(uri);

            _wampServer.Start(config => { });
            _wampServer.Start(config => { });
            _wampServer.Start(config => { });

            var connMock1 = _connections.First();
            var connMock2 = _connections.Skip(1).First();
            var connMock3 = _connections.Skip(2).First();

            var subscribeMsg = new SubscribeMessage { TopicUri = uri };

            connMock1.Object.OnSubscribe(subscribeMsg);
            connMock2.Object.OnSubscribe(subscribeMsg);
            connMock3.Object.OnSubscribe(subscribeMsg);

            connMock1.Setup(x => x.SendPublish(It.IsAny<PublishMessage>())).Callback(() => firstCalled = true);
            connMock2.Setup(x => x.SendPublish(It.IsAny<PublishMessage>())).Callback(() => secondCalled = true);
            connMock3.Setup(x => x.SendPublish(It.IsAny<PublishMessage>())).Callback(() => thirdCalled = true);

            var m = new PublishMessage
            {
                TopicUri = uri,
                Eligible = new HashSet<Guid>() { connMock2.Object.WebSocketConnectionInfo.Id }
            };

            connMock1.Object.OnPublish(m);

            Assert.IsFalse(firstCalled);
            Assert.IsTrue(secondCalled);
            Assert.IsFalse(thirdCalled);
        }
    }
}
