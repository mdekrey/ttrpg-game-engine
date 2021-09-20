using Newtonsoft.Json;
using System;

namespace GameEngine.Web.Storage
{
    public class GameStorageOptions
    {
        public Func<JsonSerializer, JsonSerializer>? ApplyJsonSerializerSettings { get; set; }

        internal JsonSerializer CreateJsonSerializer()
        {
            var result = new JsonSerializer();
            return ApplyJsonSerializerSettings?.Invoke(result) ?? result;
        }
    }
}