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

        [Test]
        public void TestPublishToAll()
        {
        }
    }
}
