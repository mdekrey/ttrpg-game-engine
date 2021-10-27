using System.Collections.Immutable;

namespace GameEngine.Generator
{
    public record PowerProfileConfig(string? Name, ImmutableList<PowerProfileConfig.ModifierChance> ModifierChances, ImmutableList<PowerProfileConfig.PowerChance> PowerChances)
    {
        public readonly static PowerProfileConfig Empty = new("Any Power", ImmutableList<ModifierChance>.Empty.Add(new("$", 1)), ImmutableList<PowerChance>.Empty.Add(new("$", 1)));

        public record ModifierChance(string Selector, double Weight);
        public record PowerChance(string Selector, double Weight);

    }
}
