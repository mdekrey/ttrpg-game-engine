using System.Collections.Immutable;

namespace GameEngine.Generator
{
    public record PowerProfileConfig(string? Name, ImmutableList<PowerProfileConfig.PowerChance> PowerChances)
    {
        public readonly static PowerProfileConfig Empty = new("Any Power", ImmutableList<PowerChance>.Empty.Add(new("$", 1)));

        public record PowerChance(string Selector, double Weight);

    }
}
