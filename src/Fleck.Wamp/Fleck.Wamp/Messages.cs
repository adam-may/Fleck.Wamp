using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public interface IWampMessage
    {
        MessageType MessageType { get; }
    }

    public class EventMessage : IWampMessage
    {
        public MessageType MessageType
        {
            get { return MessageType.Event; }
        }
        public Uri TopicUri { get; private set; }
        public object Event { get; private set; }
    }

    public class PublishMessage : IWampMessage
    {
        public MessageType MessageType
        {
            get { return MessageType.Publish; }
        }
        public Uri TopicUri { get; private set; }
        public object Event { get; private set; }
        public bool ExcludeMe { get; private set; }
        public IEnumerable<string> Exclude { get; private set; }
        public IEnumerable<string> Eligible { get; private set; }
    }

    public class UnsubscribeMessage : IWampMessage
    {
        public MessageType MessageType
        {
            get { return MessageType.Unsubscribe; }
        }
        public Uri TopicUri { get; private set; }
    }

    public class SubscribeMessage : IWampMessage
    {
        public MessageType MessageType
        {
            get { return MessageType.Subscribe; }
        }
        public Uri TopicUri { get; private set; }
    }

    public class CallErrorMessage : IWampMessage
    {
        public MessageType MessageType
        {
            get { return MessageType.CallError; }
        }
        public string CallId { get; private set; }
        public Uri ErrorUri { get; private set; }
        public string ErrorDescription { get; private set; }
        public string ErrorDetails { get; private set; }
    }

    public class CallResultMessage : IWampMessage
    {
        public MessageType MessageType
        {
            get { return MessageType.CallResult; }
        }
        public string CallId { get; private set; }
        public object[] Result { get; private set; }
    }

    public class CallMessage : IWampMessage
    {
        public MessageType MessageType
        {
            get { return MessageType.Call; }
        }
        public string CallId { get; private set; }
        public Uri ProcUri { get; private set; }
        public object[] Parameters { get; private set; }
    }

    public class PrefixMessage : IWampMessage
    {
        public MessageType MessageType
        {
            get { return MessageType.Prefix; }
        }
        public string Prefix { get; private set; }
        public Uri Uri { get; private set; }
    }

    public class WelcomeMessage : IWampMessage
    {
        public MessageType MessageType
        {
            get { return MessageType.Welcome; }
        }
        public string SessionId { get; private set; }
        public string ProtocolVersion { get; private set; }
        public string ServerIdentity { get; private set; }
    }
}
