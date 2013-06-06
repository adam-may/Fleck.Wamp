using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Fleck.Wamp.Json
{
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
            var objects = new List<object>();
            serializer.Populate(jsonObject.CreateReader(), objects);
            
            var target = GetType(jsonObject);
            var objectArray = objects.ToArray();

            var props = GetOrderedJsonProperties(target);

            foreach (var prop in props)
                SetValue(prop.Item2, target, objectArray[prop.Item1 - 1]);

            return target;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            var props = GetOrderedJsonProperties(value);

            foreach (var prop in props)
                writer.WriteValue(prop.Item2.GetValue(value));

            writer.WriteEndArray();
        }

        private IEnumerable<Tuple<int, PropertyInfo>> GetOrderedJsonProperties(object target)
        {
            return target.GetType()
                         .GetProperties()
                         .Select(p =>
                             {
                                 var a =
                                     p.GetCustomAttributes(typeof (JsonPropertyAttribute), true).First() as
                                     JsonPropertyAttribute;
                                 return Tuple.Create(a.Order, p);
                             })
                         .OrderBy(p => p.Item1);
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

        private static void SetValue(PropertyInfo p, object target, object value)
        {
            p.SetValue(target, p.PropertyType.IsEnum ? Enum.Parse(p.PropertyType, value.ToString()) : value);
        }

    }
}
