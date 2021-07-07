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

        public static double ExpectedProficiencyModifier(int level) => level;
        public static double ExpectedPrimaryAbilityModifier(int level) => 4 + level / 4;
    }
}
