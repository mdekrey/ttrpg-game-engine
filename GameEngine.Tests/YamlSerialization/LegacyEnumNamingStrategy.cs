using Newtonsoft.Json;
using System;

namespace GameEngine.Tests.YamlSerialization
{
    internal class LegacyEnumNamingStrategy : JsonConverter
    {
        public override bool CanConvert(System.Type objectType)
        {
            Type t = (IsNullableType(objectType))
                ? Nullable.GetUnderlyingType(objectType)!
                : objectType;

            return t.IsEnum;
        }

        public override object? ReadJson(JsonReader reader, System.Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                if (!IsNullableType(objectType))
                {
                    throw new NotSupportedException();
                }

                return null;
            }

            var enumText = reader.Value!.ToString()!;
            return Enum.Parse(objectType, enumText);
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {

            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            Enum e = (Enum)value;
            writer.WriteValue(e.ToString("g"));
        }

        private static bool IsNullableType(Type t)
        {
            return (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>));
        }
    }
}
