using GameEngine.Rules;
using System;
using System.Collections.Immutable;

namespace GameEngine
{
    public record MonsterRoleTemplate(string Name, int InitiativeBonus, int HitPoints, int ArmorClassBase, int OtherDefenses, int AttackBonus, int AttackOtherBonus)
    {
        public static readonly MonsterRoleTemplate Skirmisher =
            new MonsterRoleTemplate(Name: "Skirmisher", InitiativeBonus: 2, HitPoints: 8, ArmorClassBase: 14, OtherDefenses: 12, AttackBonus: 5, AttackOtherBonus: 3);
        public static readonly MonsterRoleTemplate Brute =
            new MonsterRoleTemplate(Name: "Brute", InitiativeBonus: 0, HitPoints: 10, ArmorClassBase: 12, OtherDefenses: 12, AttackBonus: 3, AttackOtherBonus: 1);
        public static readonly MonsterRoleTemplate Soldier =
            new MonsterRoleTemplate(Name: "Soldier", InitiativeBonus: 2, HitPoints: 8, ArmorClassBase: 16, OtherDefenses: 12, AttackBonus: 7, AttackOtherBonus: 5);
        public static readonly MonsterRoleTemplate Lurker =
            new MonsterRoleTemplate(Name: "Lurker", InitiativeBonus: 4, HitPoints: 6, ArmorClassBase: 14, OtherDefenses: 12, AttackBonus: 5, AttackOtherBonus: 3);
        public static readonly MonsterRoleTemplate Controller =
            new MonsterRoleTemplate(Name: "Controller", InitiativeBonus: 0, HitPoints: 8, ArmorClassBase: 14, OtherDefenses: 12, AttackBonus: 5, AttackOtherBonus: 4);
        public static readonly MonsterRoleTemplate Artillery =
            new MonsterRoleTemplate(Name: "Artillery", InitiativeBonus: 0, HitPoints: 6, ArmorClassBase: 12, OtherDefenses: 12, AttackBonus: 7, AttackOtherBonus: 5);

        public static int PrimaryStat(int level) => 3 + level / 4;
        public static int SecondaryStat(int level) => 1 + (level + 2) / 4;

        public CharacterStats GetStatsFor(int level, CharacterAbilities abilities)
        {
            var proficiency = CombatExpectations.ExpectedProficiencyModifier(level);
            return new CharacterStats(
                Level: level,
                Abilities: abilities,
                ArmorClass: ArmorClassBase + level,
                FortitudeSave: Math.Max(abilities.Strength, abilities.Constitution) + proficiency,
                ReflexSave: Math.Max(abilities.Dexterity, abilities.Intelligence) + proficiency,
                WillSave: Math.Max(abilities.Wisdom, abilities.Charisma) + proficiency,
                MaxHitPoints: HitPoints * (level + 1) + abilities.Constitution * 2 + 10
            );
        }
    }
}
