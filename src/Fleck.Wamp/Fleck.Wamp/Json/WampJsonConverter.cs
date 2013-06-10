using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Fleck.Wamp.Json
{
    /// <summary>
    /// Based upon http://stackoverflow.com/questions/6997502/property-based-type-resolution-in-json-net
    /// </summary>
    public class WampJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Crappy way to do it, but the people who wrote the WAMP spec must have not been 
            // thinking this all the way through.
            // As it's an array, deserializing to any programming object model is a pain in 
            // the butt.
            // We first deserialize to and object array, then we'll set the properties on the 
            // target object to match
            var jsonObject = JArray.Load(reader);
            var target = GetType(jsonObject);
            var r = jsonObject.CreateReader();

            var properties = GetOrderedProperties(target);

            // The first item is the opening read marks, so we read past this
            if (r.TokenType == JsonToken.None)
                r.Read();

            if (r.TokenType != JsonToken.StartArray)
                throw new JsonSerializationException("Invalid message. Needs to be a JSON Array");

            foreach (var property in properties)
            {
                var value = new ReflectionValueProvider(property);

                switch (r.TokenType)
                {
                    case JsonToken.None:
                    case JsonToken.EndArray:
                    case JsonToken.EndObject:
                        // Setting the value to null here is like setting it to default(Type) as it internally 
                        // handles Value vs Reference Types
                        value.SetValue(target, null);
                        continue;
                }

                if (!r.Read())
                    throw new JsonSerializationException("Problem deserializing");

                if (property.PropertyType == typeof (object[]))
                {
                    if (r.TokenType == JsonToken.StartObject)
                    {
                        value.SetValue(target, new object[] {serializer.Deserialize<Dictionary<string, object>>(r)});
                    }
                    else
                    {
                        var objs = new List<object>();
                        if (r.TokenType != JsonToken.EndArray)
                            objs.Add(GetObjectArrayType(serializer, r));
                        while (r.Read())
                            if (IsWritableTokenType(r.TokenType))
                                objs.Add(GetObjectArrayType(serializer, r));
                        value.SetValue(target, ConvertValueToType(property, objs.ToArray()));
                    }
                }
                else
                {
                    var val = serializer.Deserialize(r);
                    value.SetValue(target, ConvertValueToType(property, val));
                }
            }

            return target;
        }

        private static object GetObjectArrayType(JsonSerializer serializer, JsonReader r)
        {
            return r.TokenType == JsonToken.StartObject
                       ? serializer.Deserialize<Dictionary<string, object>>(r)
                       : serializer.Deserialize(r);
        }

        private static bool IsWritableTokenType(JsonToken token)
        {
            return token == JsonToken.Boolean ||
                   token == JsonToken.Date ||
                   token == JsonToken.Float ||
                   token == JsonToken.Integer ||
                   token == JsonToken.PropertyName ||
                   token == JsonToken.String ||
                   token == JsonToken.StartObject ||
                   token == JsonToken.StartArray;
        }

        public override void WriteJson(JsonWriter writer, object target, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            var properties = GetOrderedProperties(target);

            foreach (var property in properties)
            {
                var value = new ReflectionValueProvider(property);

                if (property.PropertyType == typeof (object[]))
                {
                    foreach (var v in value.GetValue(target) as object[])
                    {
                        serializer.Serialize(writer, v);
                    }
                }
                else if (property.PropertyType == typeof(IEnumerable<Guid>) || Nullable.GetUnderlyingType(property.PropertyType) != null)
                {
                    var val = value.GetValue(target);
                    if (val != null)
                        serializer.Serialize(writer, val);
                }
                else
                {
                    serializer.Serialize(writer, value.GetValue(target));
                }
            }
            writer.WriteEndArray();
        }

        private static IEnumerable<PropertyInfo> GetOrderedProperties(object target)
        {
            return target.GetType()
                         .GetProperties()
                         .Select(p =>
                             {
                                 var attr = p.GetCustomAttribute(typeof(JsonPropertyAttribute), true) as JsonPropertyAttribute;
                                 return attr != null ? new { attr.Order, PropertyInfo = p } : null;
                             })
                         .OrderBy(p => p.Order)
                         .Select(p => p.PropertyInfo);
        }

        private static IWampMessage GetType(IEnumerable<JToken> jArray)
        {
            var messageTypeId = jArray.First().Value<int>();
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
                default: throw new ArgumentException("Invalid message type", "jArray");
            }
        }

        private static object ConvertValueToType(PropertyInfo propertyInfo, object value)
        {
            if (value == null)
                return null;

            if (propertyInfo == typeof(string) || propertyInfo == typeof(object[]))
                return value;

            if (propertyInfo.PropertyType.IsEnum)
                return Enum.Parse(propertyInfo.PropertyType, value.ToString());

            if (propertyInfo.PropertyType == typeof(Uri))
                return new Uri(value.ToString());

            if (propertyInfo.PropertyType == typeof(Guid))
                return new Guid(value.ToString());

            return Convert.ChangeType(value, propertyInfo.PropertyType);
        }
    }
}
