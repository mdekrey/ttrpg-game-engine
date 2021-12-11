using GameEngine.Generator.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.IO;
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
                    new Newtonsoft.Json.Converters.StringEnumConverter(),
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

        [return: System.Diagnostics.CodeAnalysis.MaybeNull]
        public static T DeserializeYaml<T>(string v)
        {
            using var sr = new System.IO.StringReader(v);
            var yamlObject = YamlDeserializer.Deserialize(sr);
            var token = Newtonsoft.Json.Linq.JToken.FromObject(yamlObject!, JsonSerializer);
            return token.ToObject<T>(JsonSerializer);
        }
    }
}
