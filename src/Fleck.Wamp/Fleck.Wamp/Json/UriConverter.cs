using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fleck.Wamp.Json
{
    public class UriConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (null == value)
            {
                writer.WriteNull();
                return;
            }

            if (value is Uri)
            {
                writer.WriteValue(((Uri)value).OriginalString);
                return;
            }

            throw new InvalidOperationException("Unhandled case for UriConverter. Check to see if this converter has been applied to the wrong serialization type.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                return new Uri((string)reader.Value);
            }

            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            throw new InvalidOperationException("Unhandled case for UriConverter. Check to see if this converter has been applied to the wrong serialization type.");
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Uri);
        }
    }
}
