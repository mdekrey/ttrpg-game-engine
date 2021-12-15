using GameEngine.Generator.Modifiers;
using JsonSubTypes;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace GameEngine.Generator.Serialization
{
    public static class ProfileSerialization
    {
        public static IEnumerable<JsonConverter> GetJsonConverters()
        {
            yield return new GameDiceExpressionConverter();
            yield return GetConverter<IEffectModifier>();
            yield return GetConverter<IAttackTargetModifier>();
            yield return GetConverter<IEffectTargetModifier>();
            yield return GetConverter<IAttackModifier>();
            yield return GetConverter<IPowerModifier>();
            yield return GetConverter<Modifiers.BoostFormula.Boost>();
            yield return GetConverter<Modifiers.SkirmishFormula.SkirmishMovement>();
            yield return GetConverter<Modifiers.ConditionFormula.Condition>();
            yield return GetConverter<Modifiers.MovementControlFormula.MovementControl>();
        }

        private static JsonConverter GetConverter<T>()
        {
            var builder = JsonSubtypesConverterBuilder.Of<T>("Name").SerializeDiscriminatorProperty(addDiscriminatorFirst: true);

            var entries = from type in typeof(T).Assembly.GetTypes()
                          where typeof(T).IsAssignableFrom(type) && !typeof(RewritePowerModifier).IsAssignableFrom(type)
                          where !type.IsAbstract
                          select type;

            foreach (var entry in entries)
            {
                builder.RegisterSubtype(entry, ModifierNameAttribute.GetName(entry));
            }

            return builder.Build();
        }
    }
}
