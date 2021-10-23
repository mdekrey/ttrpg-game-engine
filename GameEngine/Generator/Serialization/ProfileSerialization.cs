using GameEngine.Generator.Modifiers;
using JsonSubTypes;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace GameEngine.Generator.Serialization
{
    public static class ProfileSerialization
    {
        const string version = "1";

        public static IEnumerable<JsonConverter> GetJsonConverters()
        {
            yield return GetConverter<ITargetEffectModifier>();
            yield return GetConverter<IAttackModifier>();
            yield return GetConverter<IPowerModifier>();
            yield return GetConverter<Modifiers.BoostFormula.Boost>();
            yield return GetConverter<Modifiers.SkirmishFormula.SkirmishMovement>();
            yield return GetConverter<Modifiers.ConditionFormula.Condition>();
            yield return GetConverter<Modifiers.MovementControlFormula.MovementControl>();
        }

        private static JsonConverter GetConverter<T>()
        {
            // TODO - older versions
            var builder = JsonSubtypesConverterBuilder.Of<T>("$$modType").SerializeDiscriminatorProperty(addDiscriminatorFirst: true);

            var entries = from type in typeof(T).Assembly.GetTypes()
                          where typeof(T).IsAssignableFrom(type)
                          where !type.IsAbstract
                          select type;

            foreach (var entry in entries)
            {
                builder.RegisterSubtype(entry, entry.Name + ".v" + version);
            }

            return builder.Build();
        }
    }
}
