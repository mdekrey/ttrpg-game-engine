using System;
using System.Collections.Generic;
using System.Linq;

namespace GameEngine.Rules
{
    public record CharacterStats(
        int Level,
        CharacterAbilities Abilities,
        int ArmorClass,
        int FortitudeSave,
        int ReflexSave,
        int WillSave,
        int MaxHitPoints
    )
    {
        public static readonly CharacterStats Default = CharacterAbilities.FromStandardArray(CombatExpectations.StandardArrayBy4Levels[0], new[] {
                Ability.Strength,
                Ability.Dexterity,
                Ability.Constitution,
                Ability.Wisdom,
                Ability.Intelligence,
                Ability.Charisma,
            }) switch {
                var abilities => new CharacterStats(
                    Level: 1, 
                    Abilities: abilities,
                    ArmorClass: 15,
                    FortitudeSave: 12,
                    ReflexSave: 12,
                    WillSave: 12,
                    MaxHitPoints: 12 + abilities.Constitution
                )
            };
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
        public static readonly CharacterAbilities Empty = new(0, 0, 0, 0, 0, 0);
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

        public static CharacterAbilities operator +(CharacterAbilities lhs, CharacterAbilities rhs) =>
            new CharacterAbilities(
                lhs.Strength + rhs.Strength,
                lhs.Constitution + rhs.Constitution,
                lhs.Dexterity + rhs.Dexterity,
                lhs.Intelligence + rhs.Intelligence,
                lhs.Wisdom + rhs.Wisdom,
                lhs.Charisma + rhs.Charisma
            );

        public static CharacterAbilities operator -(CharacterAbilities lhs, CharacterAbilities rhs) =>
            new CharacterAbilities(
                lhs.Strength - rhs.Strength,
                lhs.Constitution - rhs.Constitution,
                lhs.Dexterity - rhs.Dexterity,
                lhs.Intelligence - rhs.Intelligence,
                lhs.Wisdom - rhs.Wisdom,
                lhs.Charisma - rhs.Charisma
            );

        public static CharacterAbilities operator -(CharacterAbilities orig) =>
            new CharacterAbilities(
                -orig.Strength,
                -orig.Constitution,
                -orig.Dexterity,
                -orig.Intelligence,
                -orig.Wisdom,
                -orig.Charisma
            );

        public static int operator *(CharacterAbilities lhs, CharacterAbilities rhs) =>
            lhs.Strength * rhs.Strength+
            lhs.Constitution * rhs.Constitution+
            lhs.Dexterity * rhs.Dexterity+
            lhs.Intelligence * rhs.Intelligence+
            lhs.Wisdom * rhs.Wisdom+
            lhs.Charisma * rhs.Charisma;

        public static CharacterAbilities operator *(CharacterAbilities lhs, int rhs) =>
            new CharacterAbilities(
                lhs.Strength * rhs,
                lhs.Constitution * rhs,
                lhs.Dexterity * rhs,
                lhs.Intelligence * rhs,
                lhs.Wisdom * rhs,
                lhs.Charisma * rhs
            );
    }
}