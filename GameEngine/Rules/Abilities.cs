using System.Collections.Immutable;

namespace GameEngine.Rules
{
    public static class Abilities
    {
        public static readonly ImmutableList<Ability> All = new[] {
            Ability.Strength,
            Ability.Dexterity,
            Ability.Constitution,
            Ability.Wisdom,
            Ability.Intelligence,
            Ability.Charisma,
        }.ToImmutableList();
    }
}
