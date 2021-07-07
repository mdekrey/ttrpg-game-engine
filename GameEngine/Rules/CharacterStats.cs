using System;
using System.Collections.Generic;
using System.Linq;

namespace GameEngine.Rules
{
    public record CharacterStats(
        int Level,
        CharacterAbilities Abilities
    )
    {
        public static readonly CharacterStats Default = new CharacterStats(1, CharacterAbilities.FromStandardArray(CombatExpectations.StandardArrayBy4Levels[0], new[] {
            Ability.Strength,
            Ability.Dexterity,
            Ability.Constitution,
            Ability.Wisdom,
            Ability.Intelligence,
            Ability.Charisma,
        }));

        internal int GetArmorClass() =>
            // TODO - equipment, class bonuses, feats, enhancements, etc.
            10 + Abilities[Ability.Dexterity] + 2;
    }

    public record CharacterAbilities(
        int Strength,
        int Constitution,
        int Dexterity,
        int Intelligence,
        int Wisdom,
        int Charisma
    )
    {
        public int this[Ability ability] =>
            ability switch
            {
                Ability.Strength => Strength,
                Ability.Constitution => Constitution,
                Ability.Dexterity => Dexterity,
                Ability.Intelligence => Intelligence,
                Ability.Wisdom => Wisdom,
                Ability.Charisma => Charisma,

                >= Ability.Max or < 0 => throw new NotSupportedException(),
            };

        public static CharacterAbilities FromStandardArray(IReadOnlyList<int> standardArray, IReadOnlyList<Ability> abilities)
        {
            var lookup = abilities.Select((a, i) => (a, i)).ToDictionary(kvp => kvp.a, kvp => kvp.i);
            return new CharacterAbilities(
                Strength: standardArray[lookup[Ability.Strength]],
                Constitution: standardArray[lookup[Ability.Constitution]],
                Dexterity: standardArray[lookup[Ability.Dexterity]],
                Intelligence: standardArray[lookup[Ability.Intelligence]],
                Wisdom: standardArray[lookup[Ability.Wisdom]],
                Charisma: standardArray[lookup[Ability.Charisma]]
            );
        }
    }
}