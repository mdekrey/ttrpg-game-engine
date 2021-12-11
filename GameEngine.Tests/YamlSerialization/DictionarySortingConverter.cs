using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GameEngine.Tests.YamlSerialization
{
    internal class DictionarySortingConverter : JsonConverter
    {
        public override bool CanConvert(System.Type objectType)
        {
            return objectType.GetInterfaces().Any(iface => iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IDictionary<,>));
        }

        public override bool CanRead => false;

        public override object? ReadJson(JsonReader reader, System.Type objectType, object? existingValue, JsonSerializer serializer)
        {
            throw new System.NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            var keyValuePairs = ((IEnumerable)value).OfType<dynamic>().Select(kv => (Key: (string)kv.Key.ToString(), Value: (object)kv.Value)).ToArray();
            writer.WriteStartObject();
            foreach (var kv in keyValuePairs.OrderBy(kv => kv.Key))
            {
                writer.WritePropertyName(kv.Key);
                serializer.Serialize(writer, kv.Value);
            }
            writer.WriteEndObject();
        }
    }
}
