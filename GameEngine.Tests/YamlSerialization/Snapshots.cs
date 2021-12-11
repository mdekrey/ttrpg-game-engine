using GameEngine.Generator.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;

namespace GameEngine.Tests.YamlSerialization
{
    public static class Snapshots
    {
        private static readonly YamlDotNet.Serialization.ISerializer Serializer =
            new YamlDotNet.Serialization.SerializerBuilder()
                .DisableAliases()
                //.WithTypeConverter(new DictionaryTypeConverter())
                .Build();
        private static readonly Deserializer YamlDeserializer = new YamlDotNet.Serialization.Deserializer();
        private static readonly JsonSerializer JsonSerializer;

        static Snapshots()
        {
            JsonSerializer = new Newtonsoft.Json.JsonSerializer()
            {
                Converters =
                {
                    new LegacyEnumNamingStrategy(),
                    new GameDiceExpressionConverter(),
                    new DictionarySortingConverter(),
                },
                NullValueHandling = NullValueHandling.Ignore,
            };
            foreach (var converter in ProfileSerialization.GetJsonConverters())
                JsonSerializer.Converters.Add(converter);
        }

        public static string SerializeToYaml(object target)
        {
            var json = JToken.FromObject(target, JsonSerializer).ToString();
            using var sr = new StringReader(json);
            return Serializer.Serialize(YamlDeserializer.Deserialize(sr)!);
        }
    }

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
                writer.WriteValue(kv.Value);
            }
            writer.WriteEndObject();
        }
    }
}
