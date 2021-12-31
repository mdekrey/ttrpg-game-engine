using GameEngine.Rules;
using Newtonsoft.Json;
using System;

namespace GameEngine.Generator.Serialization
{
    public class GameDiceExpressionConverter : JsonConverter<GameDiceExpression>
    {
        public override GameDiceExpression? ReadJson(JsonReader reader, System.Type objectType, GameDiceExpression? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var s = (string?)reader.Value;

            if (s == null) return null;

            try
            {
                return GameDiceExpression.Parse(s);
            }
            catch (Exception ex)
            {
                throw new JsonSerializationException("Exception encountered parsing GameDiceExpression", ex);
            }

        }

        public override void WriteJson(JsonWriter writer, GameDiceExpression? value, JsonSerializer serializer)
        {
            writer.WriteValue(value?.ToString());
        }
    }
}
