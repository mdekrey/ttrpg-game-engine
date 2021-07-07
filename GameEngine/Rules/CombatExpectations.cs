using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngine
{
    public static class CombatExpectations
    {
        public enum ActionType
        {
            Major,
            Minor,
            Free,
            Reaction
        }

        public const int averageRoundsPerCombat = 5;
        public const int averageCombatsPerDay = 4;

        public const double averagePrimaryWeaponDamage = 4.5;
        public const double averageSecondaryWeaponDamage = 3.5;

        public static readonly IReadOnlyList<IReadOnlyList<int>> StandardArrayBy4Levels = new[]
        {
            new[] { 3, 2, 1, 1, 0, 0 },
            new[] { 4, 2, 1, 1, 0, 0 },
            new[] { 5, 2, 1, 1, 0, 0 },
            new[] { 5, 3, 1, 1, 0, 0 },
            new[] { 5, 3, 2, 1, 0, 0 },
            new[] { 5, 3, 3, 1, 0, 0 },
            new[] { 5, 4, 3, 1, 0, 0 },
            new[] { 5, 4, 4, 1, 0, 0 },
        };

        public static double ExpectedProficiencyModifier(int level) => level;
        public static double ExpectedAbilityModifier(int level, Index index) => StandardArrayBy4Levels[level / 4][index];
    }
}
