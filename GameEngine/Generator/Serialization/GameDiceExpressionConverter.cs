using GameEngine.Rules;
using Newtonsoft.Json;

namespace GameEngine.Generator.Serialization
{
    public class GameDiceExpressionConverter : JsonConverter<GameDiceExpression>
    {
        public override GameDiceExpression? ReadJson(JsonReader reader, System.Type objectType, GameDiceExpression? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var s = (string?)reader.Value;

            if (s == null) return null;

            return GameDiceExpression.Parse(s);

        }

        public override void WriteJson(JsonWriter writer, GameDiceExpression? value, JsonSerializer serializer)
        {
            writer.WriteValue(value?.ToString());
        }
    }
}
