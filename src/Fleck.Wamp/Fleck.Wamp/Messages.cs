using System;
using System.Collections.Generic;
using Fleck.Wamp.Json;
using Newtonsoft.Json;

namespace Fleck.Wamp
{
    public enum MessageType
    {
        Welcome = 0,
        Prefix,
        Call,
        CallResult,
        CallError,
        Subscribe,
        Unsubscribe,
        Publish,
        Event
    }

    [JsonConverter(typeof(WampJsonConverter))]
    public interface IWampMessage
    {
        MessageType MessageType { get; }
    }

    public class WampMessage : IWampMessage
    {
        [JsonProperty(Order = 1)]
        public MessageType MessageType { get; internal set; }
    }

    public class EventMessage : WampMessage
    {
        public EventMessage()
        {
            MessageType = MessageType.Event;
        }
        public Uri TopicUri { get; set; }
        public object Event { get; set; }
    }

    public class PublishMessage : WampMessage
    {
        public PublishMessage()
        {
            MessageType = MessageType.Publish;
        }
        public Uri TopicUri { get; set; }
        public object Event { get; set; }
        public bool ExcludeMe { get; set; }
        public IEnumerable<string> Exclude { get; set; }
        public IEnumerable<string> Eligible { get; set; }
    }

    public class UnsubscribeMessage : WampMessage
    {
        public UnsubscribeMessage()
        {
            MessageType = MessageType.Unsubscribe;
        }
        public Uri TopicUri { get; set; }
    }

    public class SubscribeMessage : WampMessage
    {
        public SubscribeMessage()
        {
            MessageType = MessageType.Subscribe;
        }
        public Uri TopicUri { get; set; }
    }

    public class CallErrorMessage : WampMessage
    {
        public CallErrorMessage()
        {
            MessageType = MessageType.CallError;
        }
        public string CallId { get; set; }
        public Uri ErrorUri { get; set; }
        public string ErrorDescription { get; set; }
        public string ErrorDetails { get; set; }
    }

    public class CallResultMessage : WampMessage
    {
        public CallResultMessage()
        {
            MessageType = MessageType.CallResult;
        }
        public string CallId { get; set; }
        public object[] Result { get; set; }
    }

    public class CallMessage : WampMessage
    {
        public CallMessage()
        {
            MessageType = MessageType.Call;
        }
        public string CallId { get; set; }
        public Uri ProcUri { get; set; }
        public object[] Parameters { get; set; }
    }

    public class PrefixMessage : WampMessage
    {
        public PrefixMessage()
        {
            MessageType = MessageType.Prefix;
        }
        public string Prefix { get; set; }
        public Uri Uri { get; set; }
    }

    [JsonConverter(typeof(WampJsonConverter))]
    public class WelcomeMessage : WampMessage
    {
        public WelcomeMessage()
        {
            MessageType = MessageType.Welcome;
        }
        [JsonProperty(Order = 2)]
        public Guid SessionId { get; set; }
        [JsonProperty(Order = 3)]
        public long ProtocolVersion { get; set; }
        [JsonProperty(Order = 4)]
        public string ServerIdentity { get; set; }
    }
}
