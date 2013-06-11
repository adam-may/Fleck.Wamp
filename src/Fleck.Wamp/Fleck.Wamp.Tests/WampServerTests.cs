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
            Assert.AreEqual(1, _wampServer.ProtocolVersion);
        }

        [Test]
        public void TestServerIdentityIsCorrect()
        {
            Assert.AreEqual("Fleck.Wamp/0.9.6", _wampServer.ServerIdentity);
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
    }
}
