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

    [JsonConverter(typeof(WampJsonConverter))]
    public class WampMessage : IWampMessage
    {
        [JsonProperty(Order = 1)]
        public MessageType MessageType { get; internal set; }
    }

    [JsonConverter(typeof(WampJsonConverter))]
    public class EventMessage : WampMessage
    {
        public EventMessage()
        {
            MessageType = MessageType.Event;
        }
        [JsonConverter(typeof(UriConverter))]
        [JsonProperty(Order = 2)]
        public Uri TopicUri { get; set; }
        [JsonProperty(Order = 3)]
        public object[] Event { get; set; }
    }

    [JsonConverter(typeof(WampJsonConverter))]
    public class PublishMessage : WampMessage
    {
        public PublishMessage()
        {
            MessageType = MessageType.Publish;
        }
        [JsonConverter(typeof(UriConverter))]
        [JsonProperty(Order = 2)]
        public Uri TopicUri { get; set; }
        [JsonProperty(Order = 3)]
        public object[] Event { get; set; }
        [JsonProperty(Order = 4)]
        public bool? ExcludeMe { get; set; }
        [JsonProperty(Order = 5)]
        public IEnumerable<Guid> Exclude { get; set; }
        [JsonProperty(Order = 6)]
        public IEnumerable<Guid> Eligible { get; set; }
    }

    [JsonConverter(typeof(WampJsonConverter))]
    public class UnsubscribeMessage : WampMessage
    {
        public UnsubscribeMessage()
        {
            MessageType = MessageType.Unsubscribe;
        }
        [JsonConverter(typeof(UriConverter))]
        [JsonProperty(Order = 2)]
        public Uri TopicUri { get; set; }
    }

    [JsonConverter(typeof(WampJsonConverter))]
    public class SubscribeMessage : WampMessage
    {
        public SubscribeMessage()
        {
            MessageType = MessageType.Subscribe;
        }
        [JsonConverter(typeof(UriConverter))]
        [JsonProperty(Order = 2)]
        public Uri TopicUri { get; set; }
    }

    [JsonConverter(typeof(WampJsonConverter))]
    public class CallErrorMessage : WampMessage
    {
        public CallErrorMessage()
        {
            MessageType = MessageType.CallError;
        }
        [JsonProperty(Order = 2)]
        public string CallId { get; set; }
        [JsonConverter(typeof(UriConverter))]
        [JsonProperty(Order = 3)]
        public Uri ErrorUri { get; set; }
        [JsonProperty(Order = 4)]
        public string ErrorDescription { get; set; }
        [JsonProperty(Order = 5)]
        public object[] ErrorDetails { get; set; }
    }

    [JsonConverter(typeof(WampJsonConverter))]
    public class CallResultMessage : WampMessage
    {
        public CallResultMessage()
        {
            MessageType = MessageType.CallResult;
        }
        [JsonProperty(Order = 2)]
        public string CallId { get; set; }
        [JsonProperty(Order = 3)]
        public object[] Result { get; set; }
    }

    [JsonConverter(typeof(WampJsonConverter))]
    public class CallMessage : WampMessage
    {
        public CallMessage()
        {
            MessageType = MessageType.Call;
        }
        [JsonProperty(Order = 2)]
        public string CallId { get; set; }
        [JsonConverter(typeof(UriConverter))]
        [JsonProperty(Order = 3)]
        public Uri ProcUri { get; set; }
        [JsonProperty(Order = 4)]
        public object[] Parameters { get; set; }
    }

    [JsonConverter(typeof(WampJsonConverter))]
    public class PrefixMessage : WampMessage
    {
        public PrefixMessage()
        {
            MessageType = MessageType.Prefix;
        }
        [JsonProperty(Order = 2)]
        public string Prefix { get; set; }
        [JsonConverter(typeof(UriConverter))]
        [JsonProperty(Order = 3)]
        public Uri Uri { get; set; }
    }

    [JsonConverter(typeof(WampJsonConverter))]
    public class WelcomeMessage : WampMessage
    {
        public WelcomeMessage()
        {
            MessageType = MessageType.Welcome;
        }
        [JsonConverter(typeof(GuidConverter))]
        [JsonProperty(Order = 2)]
        public Guid SessionId { get; set; }
        [JsonProperty(Order = 3)]
        public long ProtocolVersion { get; set; }
        [JsonProperty(Order = 4)]
        public string ServerIdentity { get; set; }
    }
}
