using GameEngine.Rules;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using YamlDotNet.Serialization;

namespace GameEngine.Tests
{
    public class PowersYamlShould
    {
        private readonly JObject content;
        private readonly JsonSerializer serializer;

        public PowersYamlShould()
        {
            using var stream = typeof(PowersYamlShould).Assembly.GetManifestResourceStream($"{typeof(PowersYamlShould).Namespace}.powers.yaml")!;
            using var reader = new StreamReader(stream);
            var deserializer = new DeserializerBuilder()
                    .Build();
            var yamlObject = deserializer.Deserialize(reader);

            // now convert the object to JSON. Simple!
            var js = new SerializerBuilder()
                .JsonCompatible()
                .Build();

            string jsonText = js.Serialize(yamlObject!);

            content = JObject.Parse(jsonText);

            serializer = new JsonSerializer()
            {
                MissingMemberHandling = MissingMemberHandling.Error,
                ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver
                {
                    NamingStrategy = new Newtonsoft.Json.Serialization.CamelCaseNamingStrategy(),
                },
                Converters =
                {
                    new Newtonsoft.Json.Converters.StringEnumConverter()
                }
            };

            var temp = JToken.FromObject(PowerFrequency.AtWill, serializer);
        }

        [Fact]
        public void ParsePowers()
        {
            var target = content["Powers"]!.ToObject<Dictionary<string, SerializedPower>>(serializer)!;
            Assert.NotNull(target);

            foreach ((string name, SerializedPower powerRecord) in target)
            {
                if (powerRecord.Level is int level)
                    Assert.True(MeetsExpectations(powerRecord.Frequency, level, powerRecord), $"{name} does not meet expectations");
            }
        }

        private bool MeetsExpectations(PowerFrequency frequency, int level, SerializedEffect power)
        {
            // TODO
            return true;
        }
    }
}
