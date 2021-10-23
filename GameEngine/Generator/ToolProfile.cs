using GameEngine.Rules;
using System.Collections.Immutable;
using System.Linq;

namespace GameEngine.Generator
{
    public record ToolProfile(ToolType Type, ToolRange Range, ImmutableList<Ability> Abilities, ImmutableList<DamageType> PreferredDamageTypes, ImmutableList<PowerProfileConfig> PowerProfileConfigs)
    {
        internal bool IsValid()
        {
            return Abilities is { Count: > 1 }
                && Abilities.Distinct().Count() == Abilities.Count
                && PreferredDamageTypes is { Count: >= 1 }
                && PowerProfileConfigs is { Count: >= 1 };
        }
    }
}
