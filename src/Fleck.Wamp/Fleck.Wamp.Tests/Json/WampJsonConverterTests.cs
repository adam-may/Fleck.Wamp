using System;

using Newtonsoft.Json;
using NUnit;
using NUnit.Framework;
using Moq;

using Fleck.Wamp.Json;
using Newtonsoft.Json.Serialization;

namespace Fleck.Wamp.Tests.Json
{
    [TestFixture]
    public class WampJsonConverterTests
    {
        private Guid _guid;

        [TestFixtureSetUp]
        public void Setup()
        {
            _guid = Guid.NewGuid();
        }

        [Test]
        public void TestWelcomeMessageSerialization()
        {
            const int protocolVersion = 1;
            const string serverIdentity = "Autobahn/0.5.1";
            var msg = String.Format("[0,\"{0}\",{1},\"{2}\"]", _guid, protocolVersion, serverIdentity);
            var o = JsonConvert.DeserializeObject<IWampMessage>(msg) as WelcomeMessage;

            Assert.IsInstanceOf(typeof(WelcomeMessage), o);
            Assert.AreEqual(MessageType.Welcome, o.MessageType);
            Assert.AreEqual(_guid, o.SessionId);
            Assert.AreEqual(protocolVersion, o.ProtocolVersion);
            Assert.AreEqual(serverIdentity, o.ServerIdentity);

            var outputMsg = JsonConvert.SerializeObject(o);

            Assert.AreEqual(msg, outputMsg);
        }

        [TestCase("calc", "http://example.com/simple/calc#")]
        [TestCase("keyvalue", "http://example.com/simple/keyvalue#")]
        [Test]
        public void TestPrefixMessageSerialization(string prefix, string uriString)
        {
            var uri = new Uri(uriString);
            var msg = String.Format("[1,\"{0}\",\"{1}\"]", prefix, uri);
            var o = JsonConvert.DeserializeObject<IWampMessage>(msg) as PrefixMessage;

            Assert.IsInstanceOf(typeof(PrefixMessage), o);
            Assert.AreEqual(MessageType.Prefix, o.MessageType);
            Assert.AreEqual(prefix, o.Prefix);
            Assert.AreEqual(uri, o.Uri);

            var outputMsg = JsonConvert.SerializeObject(o);

            Assert.AreEqual(msg, outputMsg);
        }

        [TestCase("[2,\"{0}\",\"http://example.com/api#howdy\"]")]
        [TestCase("[2,\"{0}\",\"api:add2\",23,99]")]
        [TestCase("[2,\"{0}\",\"http://example.com/api#storeMeal\",{{\"category\":\"dinner\",\"calories\":2309}}]")]
        [TestCase("[2,\"{0}\",\"http://example.com/api#woooat\",null]")]
        [TestCase("[2,\"{0}\",\"api:sum\",[9,1,3,4]]")]
        [TestCase("[2,\"{0}\",\"keyvalue:set\",\"foobar\",{{\"value1\":\"23\",\"value2\":\"singsing\",\"value3\":true,\"modified\":\"2012-03-29T10:29:16.625Z\"}}]")]
        [Test]
        public void TestCallMessageSerialization(string message)
        {
            var msg = String.Format(message, _guid);
            var o = JsonConvert.DeserializeObject<IWampMessage>(msg) as CallMessage;

            Assert.IsInstanceOf(typeof(CallMessage), o);
            Assert.AreEqual(MessageType.Call, o.MessageType);
            Assert.AreEqual(_guid.ToString(), o.CallId);

            var outputMsg = JsonConvert.SerializeObject(o);

            Assert.AreEqual(msg, outputMsg);
        }

        [TestCase("[3,\"{0}\",null]")]
        [TestCase("[3,\"{0}\",\"Awesome result ..\"]")]
        [TestCase("[3,\"{0}\",{{\"value3\":true,\"value2\":\"singsing\",\"value1\":\"23\",\"modified\":\"2012-03-29T10:29:16.625Z\"}}]")]
        [Test]
        public void TestCallResultMessageSerialization(string message)
        {
            var msg = String.Format(message, _guid);
            var o = JsonConvert.DeserializeObject<IWampMessage>(msg) as CallResultMessage;

            Assert.IsInstanceOf(typeof(CallResultMessage), o);
            Assert.AreEqual(MessageType.CallResult, o.MessageType);
            Assert.AreEqual(_guid.ToString(), o.CallId);

            var outputMsg = JsonConvert.SerializeObject(o);

            Assert.AreEqual(msg, outputMsg);
        }

        [TestCase("[4,\"{0}\",\"http://autobahn.tavendo.de/error#generic\",\"math domain error\"]")]
        [TestCase("[4,\"{0}\",\"http://example.com/error#number_too_big\",\"1001 too big for me, max is 1000\",1000]")]
        [TestCase("[4,\"{0}\",\"http://example.com/error#invalid_numbers\",\"one or more numbers are multiples of 3\",[0,3]]")]
        [Test]
        public void TestCallErrorMessageSerialization(string message)
        {
            var msg = String.Format(message, _guid);
            var o = JsonConvert.DeserializeObject<IWampMessage>(msg) as CallErrorMessage;

            Assert.IsInstanceOf(typeof(CallErrorMessage), o);
            Assert.AreEqual(MessageType.CallError, o.MessageType);
            Assert.AreEqual(_guid.ToString(), o.CallId);

            var outputMsg = JsonConvert.SerializeObject(o);

            Assert.AreEqual(msg, outputMsg);
        }

        [TestCase("[5,\"http://example.com/simple\"]")]
        [TestCase("[5,\"event:myevent1\"]")]
        [Test]
        public void TestSubscribeMessageSerialization(string message)
        {
            var msg = String.Format(message, _guid);
            var o = JsonConvert.DeserializeObject<IWampMessage>(msg) as SubscribeMessage;

            Assert.IsInstanceOf(typeof(SubscribeMessage), o);
            Assert.AreEqual(MessageType.Subscribe, o.MessageType);

            var outputMsg = JsonConvert.SerializeObject(o);

            Assert.AreEqual(msg, outputMsg);
        }

        [TestCase("[6,\"http://example.com/simple\"]")]
        [TestCase("[6,\"event:myevent1\"]")]
        [Test]
        public void TestUnsubscribeMessageSerialization(string message)
        {
            var msg = String.Format(message, _guid);
            var o = JsonConvert.DeserializeObject<IWampMessage>(msg) as UnsubscribeMessage;

            Assert.IsInstanceOf(typeof(UnsubscribeMessage), o);
            Assert.AreEqual(MessageType.Unsubscribe, o.MessageType);

            var outputMsg = JsonConvert.SerializeObject(o);

            Assert.AreEqual(msg, outputMsg);
        }

        [TestCase("[7,\"http://example.com/simple\",\"Hello, world!\"]")]
        [TestCase("[7,\"http://example.com/simple\",null]")]
        [TestCase("[7,\"http://example.com/event#myevent2\",{{\"rand\":0.091870327345758618,\"flag\":false,\"num\":23,\"name\":\"Kross\",\"created\":\"2012-03-29T10:41:09.864Z\"}}]")]
        [TestCase("[7,\"event:myevent1\",\"hello\",[\"{0}\",\"{1}\"]]")]
        [TestCase("[7,\"event:myevent1\",\"hello\",[],[\"{0}\"]]")]    
        [Test]
        public void TestPublishMessageSerialization(string message)
        {
            var guid1 = Guid.NewGuid();
            var msg = String.Format(message, _guid, guid1);
            var o = JsonConvert.DeserializeObject<IWampMessage>(msg) as PublishMessage;

            Assert.IsInstanceOf(typeof(PublishMessage), o);
            Assert.AreEqual(MessageType.Publish, o.MessageType);

            var outputMsg = JsonConvert.SerializeObject(o);

            Assert.AreEqual(msg, outputMsg);
        }

        [TestCase("[8,\"http://example.com/simple\",\"Hello, I am a simple event.\"]")]
        [TestCase("[8,\"http://example.com/simple\",null]")]
        [TestCase("[8,\"http://example.com/event#myevent2\",{{\"rand\":0.091870327345758618,\"flag\":false,\"num\":23,\"name\":\"Kross\",\"created\":\"2012-03-29T10:41:09.864Z\"}}]")]
        [Test]
        public void TestEventMessageSerialization(string message)
        {
            var msg = String.Format(message, _guid);
            var o = JsonConvert.DeserializeObject<IWampMessage>(msg) as EventMessage;

            Assert.IsInstanceOf(typeof(EventMessage), o);
            Assert.AreEqual(MessageType.Event, o.MessageType);

            var outputMsg = JsonConvert.SerializeObject(o);

            Assert.AreEqual(msg, outputMsg);
        }
    
    }
}
