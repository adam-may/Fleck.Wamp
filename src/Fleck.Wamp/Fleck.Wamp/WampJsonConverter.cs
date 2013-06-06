using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Fleck.Wamp
{
    public class WampJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsAssignableFrom(typeof (WampMessage));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Read the type of the message
            if (!reader.Read())
                throw new SerializationException("Invalid message");

            int messageType;

            if (!Int32.TryParse(reader.Value.ToString(), out messageType))
                throw new SerializationException("Invalid message type");

            var message = GetMessageInstanceFromId(messageType);

            while (reader.Read())
            {
                
            }

            return message;
        }

        private WampMessage GetMessageInstanceFromId(int messageTypeId)
        {
            switch (messageTypeId)
            {
                case 0: return new WelcomeMessage();
                case 1: return new PrefixMessage();
                case 2: return new CallMessage();
                case 3: return new CallResultMessage();
                case 4: return new CallErrorMessage();
                case 5: return new SubscribeMessage();
                case 6: return new UnsubscribeMessage();
                case 7: return new PublishMessage();
                case 8: return new EventMessage();
                default: throw new ArgumentException("Invalid message type", "messageTypeId");
            }

        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var message = value as WampMessage;

            if (message == null)
                throw new ArgumentException("value");

            writer.WriteStartArray();
            writer.WriteValue(message.MessageType);
            writer.WriteEndArray();
        }
    }
}
