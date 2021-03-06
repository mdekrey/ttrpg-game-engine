using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace GameEngine.Web.Storage
{
    public class GameStorageOptions
    {
        public Func<JsonSerializer, JsonSerializer>? ApplyJsonSerializerSettings { get; set; }

        internal JsonSerializer CreateJsonSerializer()
        {
            var result = new JsonSerializer()
            {
                ContractResolver = new DefaultContractResolver()
                {
                    NamingStrategy = new CamelCaseNamingStrategy(),
                },
                Converters =
                {
                    new Newtonsoft.Json.Converters.StringEnumConverter(),
                }
            };
            return ApplyJsonSerializerSettings?.Invoke(result) ?? result;
        }
    }
}