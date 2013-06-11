using System;
using System.Threading;
using Moq;
using NUnit.Framework;

namespace Fleck.Wamp.Tests
{
    [TestFixture]
    public class WampCommsHandlerTests
    {
        private WampCommsHandler _wampCommsHandler;
        private Mock<IWebSocketServer> _serverMock;
        private Mock<IWebSocketConnection> _connMock;
        private Mock<IWebSocketConnectionInfo> _connInfoMock;

        [TestFixtureSetUp]
        public void Setup()
        {
            _connInfoMock = new Mock<IWebSocketConnectionInfo>();
            _connMock = new Mock<IWebSocketConnection>();
            _serverMock = new Mock<IWebSocketServer>();

            _connInfoMock.SetupGet(x => x.SubProtocol).Returns("wamp");

            _connMock.SetupGet(x => x.ConnectionInfo).Returns(_connInfoMock.Object);
            _connMock.SetupAllProperties();

            _serverMock.Setup(x => x.Start(It.IsAny<Action<IWebSocketConnection>>()))
                       .Callback<Action<IWebSocketConnection>>(x => x(_connMock.Object));

            _wampCommsHandler = new WampCommsHandler(_serverMock.Object);
        }

        [Test]
        public void TestOnOpenRaisesOnOpen()
        {
            var e = new ManualResetEvent(false);
            var wasCalled = false;

            _wampCommsHandler.Start(config =>
                {
                    config.OnOpen = () =>
                    {
                        e.Set();
                        wasCalled = true;
                    };
                });

            _connMock.Object.OnOpen();
            e.WaitOne(1000);
            Assert.True(wasCalled);
        }

        [Test]
        public void TestOnCloseRaisesOnClose()
        {
            var e = new ManualResetEvent(false);
            var wasCalled = false;

            _wampCommsHandler.Start(config =>
            {
                config.OnClose = () =>
                    {
                        e.Set();
                        wasCalled = true;
                    };
            });

            _connMock.Object.OnClose();
            e.WaitOne(1000);
            Assert.True(wasCalled);
        }

        [TestCase("calc", "http://example.com/simple/calc#")]
        [TestCase("keyvalue", "http://example.com/simple/keyvalue#")]
        [Test]
        public void TestPrefixMessageRaisesOnPrefix(string prefix, string uriString)
        {
            var e = new ManualResetEvent(false);
            var wasCalled = false;
            var uri = new Uri(uriString);
            var msg = String.Format("[1,\"{0}\",\"{1}\"]", prefix, uri);
            PrefixMessage prefixMessage = null;
            _wampCommsHandler.Start(config =>
                {
                    config.OnPrefix = m =>
                        {
                            e.Set();
                            prefixMessage = m;
                            wasCalled = true;
                        };
                });

            _connMock.Object.OnMessage(msg);
            e.WaitOne(1000);
            Assert.True(wasCalled);
            Assert.IsNotNull(prefixMessage);
            Assert.AreEqual(prefix, prefixMessage.Prefix);
            Assert.AreEqual(uri, prefixMessage.Uri);
        }

        [TestCase("[2,\"{0}\",\"http://example.com/api#howdy\"]")]
        [TestCase("[2,\"{0}\",\"api:add2\",23,99]")]
        [TestCase("[2,\"{0}\",\"http://example.com/api#storeMeal\",{{\"category\":\"dinner\",\"calories\":2309}}]")]
        [TestCase("[2,\"{0}\",\"http://example.com/api#woooat\",null]")]
        [TestCase("[2,\"{0}\",\"api:sum\",[9,1,3,4]]")]
        [TestCase("[2,\"{0}\",\"keyvalue:set\",\"foobar\",{{\"value1\":\"23\",\"value2\":\"singsing\",\"value3\":true,\"modified\":\"2012-03-29T10:29:16.625Z\"}}]")]
        [Test]
        public void TestCallMessageRaisesOnCall(string message)
        {
            var e = new ManualResetEvent(false);
            var wasCalled = false;
            const string callId = "ABC123";
            var msg = String.Format(message, callId);
            CallMessage callMessage = null;
            _wampCommsHandler.Start(config =>
            {
                config.OnCall = m =>
                {
                    e.Set();
                    callMessage = m;
                    wasCalled = true;
                };
            });

            _connMock.Object.OnMessage(msg);
            e.WaitOne(1000);
            Assert.True(wasCalled);
            Assert.IsNotNull(callMessage);
            Assert.AreEqual(callId, callMessage.CallId);
        }

        [TestCase("[5,\"http://example.com/simple\"]")]
        [TestCase("[5,\"event:myevent1\"]")]
        [Test]
        public void TestSubscribeMessageRaisesOnSubscribe(string message)
        {
            var e = new ManualResetEvent(false);
            var wasCalled = false;
            SubscribeMessage subscribeMessage = null;
            _wampCommsHandler.Start(config =>
            {
                config.OnSubscribe = m =>
                {
                    e.Set();
                    subscribeMessage = m;
                    wasCalled = true;
                };
            });

            _connMock.Object.OnMessage(message);
            e.WaitOne(1000);
            Assert.True(wasCalled);
            Assert.IsNotNull(subscribeMessage);
        }

        [TestCase("[6,\"http://example.com/simple\"]")]
        [TestCase("[6,\"event:myevent1\"]")]
        [Test]
        public void TestUnsubscribeMessageRaisesOnUnsubscribe(string message)
        {
            var e = new ManualResetEvent(false);
            var wasCalled = false;
            UnsubscribeMessage unsubscribeMessage = null;
            _wampCommsHandler.Start(config =>
            {
                config.OnUnsubscribe = m =>
                {
                    e.Set();
                    unsubscribeMessage = m;
                    wasCalled = true;
                };
            });

            _connMock.Object.OnMessage(message);
            e.WaitOne(1000);
            Assert.True(wasCalled);
            Assert.IsNotNull(unsubscribeMessage);
        }

        [TestCase("[7,\"http://example.com/simple\",\"Hello, world!\"]")]
        [TestCase("[7,\"http://example.com/simple\",null]")]
        [TestCase("[7,\"http://example.com/event#myevent2\",{{\"rand\":0.091870327345758618,\"flag\":false,\"num\":23,\"name\":\"Kross\",\"created\":\"2012-03-29T10:41:09.864Z\"}}]")]
        [TestCase("[7,\"event:myevent1\",\"hello\",[\"{0}\",\"{1}\"]]")]
        [TestCase("[7,\"event:myevent1\",\"hello\",[],[\"{0}\"]]")]
        [Test]
        public void TestPublishMessageRaisesOnPublish(string message)
        {
            var e = new ManualResetEvent(false);
            var wasCalled = false;
            var guid = Guid.NewGuid();
            var guid1 = Guid.NewGuid();
            var msg = String.Format(message, guid, guid1);

            PublishMessage publishMessage = null;
            _wampCommsHandler.Start(config =>
            {
                config.OnPublish = m =>
                {
                    e.Set();
                    publishMessage = m;
                    wasCalled = true;
                };
            });

            _connMock.Object.OnMessage(msg);
            e.WaitOne(1000);
            Assert.True(wasCalled);
            Assert.IsNotNull(publishMessage);
        }

        [ExpectedException(typeof (ArgumentException))]
        [TestCase("[0,\"00000000-0000-0000-0000-000000000000\",1,\"myserver/1.0\"]")]
        [TestCase("[3,\"{0}\",null]")]
        [TestCase("[3,\"{0}\",\"Awesome result ..\"]")]
        [TestCase("[3,\"{0}\",{{\"value3\":true,\"value2\":\"singsing\",\"value1\":\"23\",\"modified\":\"2012-03-29T10:29:16.625Z\"}}]")]
        [TestCase("[4,\"{0}\",\"http://autobahn.tavendo.de/error#generic\",\"math domain error\"]")]
        [TestCase("[4,\"{0}\",\"http://example.com/error#number_too_big\",\"1001 too big for me, max is 1000\",1000]")]
        [TestCase("[4,\"{0}\",\"http://example.com/error#invalid_numbers\",\"one or more numbers are multiples of 3\",[0,3]]")]
        [TestCase("[8,\"http://example.com/simple\",\"Hello, I am a simple event.\"]")]
        [TestCase("[8,\"http://example.com/simple\",null]")]
        [TestCase("[8,\"http://example.com/event#myevent2\",{{\"rand\":0.091870327345758618,\"flag\":false,\"num\":23,\"name\":\"Kross\",\"created\":\"2012-03-29T10:41:09.864Z\"}}]")]
        [Test]
        public void TestWelcomeMessageRaisesArgumentException(string message)
        {
            const string callId = "ABC123";
            var msg = String.Format(message, callId);
            var e = new ManualResetEvent(false);
            _wampCommsHandler.Start(config => { });
            _connMock.Object.OnMessage(msg);
            e.WaitOne(1000);
        }
    }
}
