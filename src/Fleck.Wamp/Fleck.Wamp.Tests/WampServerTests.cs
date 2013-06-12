using System;
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
        private Mock<IWampConnection> _connMock;
        private Mock<IWebSocketConnectionInfo> _connInfoMock;
        private Guid _connGuid;
        private const int ProtocolVersion = 1;
        private const string ServerIdentity = "Fleck.Wamp/0.9.6";

        [TestFixtureSetUp]
        public void Setup()
        {
            _connGuid = Guid.NewGuid();

            _connInfoMock = new Mock<IWebSocketConnectionInfo>();
            _connMock = new Mock<IWampConnection>();
            _commsMock = new Mock<IWampCommsHandler>();

            _connInfoMock.SetupGet(x => x.SubProtocol).Returns("wamp");
            _connInfoMock.SetupGet(x => x.Id).Returns(_connGuid);

            _connMock.SetupGet(x => x.WebSocketConnectionInfo).Returns(_connInfoMock.Object);
            _connMock.SetupAllProperties();

            _commsMock.Setup(x => x.Start(It.IsAny<Action<IWampConnection>>()))
                      .Callback<Action<IWampConnection>>(x => x(_connMock.Object));

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

            _connMock.Setup(x => x.SendWelcome(It.IsAny<WelcomeMessage>()))
                     .Callback<WelcomeMessage>(x => welcomeMsg = x);

            _connMock.Object.OnOpen();

            Assert.IsNotNull(welcomeMsg);
            Assert.IsInstanceOf<WelcomeMessage>(welcomeMsg);
            Assert.AreEqual(MessageType.Welcome, welcomeMsg.MessageType);
            Assert.AreEqual(1, welcomeMsg.ProtocolVersion);
            Assert.AreEqual(_connGuid, welcomeMsg.SessionId);
            Assert.AreEqual(ServerIdentity, welcomeMsg.ServerIdentity);
        }

        [Test]
        public void TestAddRemovePrefix()
        {
            const string prefix = "calc";
            var uri = new Uri("http://example.com/simple/calc#");

            var msg = new PrefixMessage {Prefix = prefix, Uri = uri};

            _wampServer.Start(config => { });

            _connMock.Object.OnPrefix(msg);

            Assert.IsTrue(_wampServer.Prefixes.ContainsKey(_connGuid));
            Assert.IsTrue(_wampServer.Prefixes[_connGuid].ContainsKey(prefix));
            Assert.AreEqual(uri, _wampServer.Prefixes[_connGuid][prefix]);

            _connMock.Object.OnClose();

            Assert.IsFalse(_wampServer.Prefixes.ContainsKey(_connGuid));
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

            _connMock.Object.OnSubscribe(msg);

            Assert.IsTrue(_wampServer.Subscriptions.ContainsKey(uri));
            Assert.IsTrue(_wampServer.Subscriptions[uri].Contains(_connGuid));
        }
    }
}
