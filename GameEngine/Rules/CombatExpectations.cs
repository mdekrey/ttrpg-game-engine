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

        /*I analyzed 4e weapons. In general:
Simple weapons baseline 4.5 damage per round.
Military weapons baseline 5.5 damage per round.
Superior weapons baseline 6.5 damage per round.
2-handed weapons get +1 damage per round. (Versatile doesn't cost anything but gives the same benefit.)
Reach costs 1 damage.
High Crit costs 1 damage.
+1 to hit costs 1 damage.
Off-hand costs 1 damage.
Heavy thrown costs 1 damage. (Because it is rare for a ranged weapon to be able to use Strength, I guess?)
        */

        public const double averagePrimaryWeaponDamage = 5.5;
        public const double averageSecondaryWeaponDamage = 4.5;

        public static readonly IReadOnlyList<(string Low, string Medium, string High)> standardDamage = new[]
        {
            ( Low: "1d6 + 3", Medium: "1d10 + 3", High: "2d6 + 3" ),
            ( Low: "1d6 + 4", Medium: "1d10 + 4", High: "2d8 + 4" ),
            ( Low: "1d8 + 5", Medium: "2d6 + 5", High: "2d8 + 5" ),
            ( Low: "1d8 + 5", Medium: "2d6 + 5", High: "3d6 + 5" ),
            ( Low: "1d10 + 6", Medium: "2d8 + 6", High: "3d6 + 6" ),
            ( Low: "1d10 + 7", Medium: "2d8 + 7", High: "3d8 + 7" ),
            ( Low: "2d6 + 7", Medium: "3d6 + 8", High: "3d8 + 7" ),
            ( Low: "2d6 + 8", Medium: "3d6 + 8", High: "4d6 + 8" ),
            ( Low: "2d8 + 9", Medium: "3d8 + 9", High: "4d6 + 9" ),
            ( Low: "2d8 + 10", Medium: "3d8 + 10", High: "4d8 + 10" ),
        };
        public static readonly IReadOnlyList<(string Low, string Medium, string High)> limitedDamage = new[]
        {
            ( Low: "3d6 + 3", Medium: "2d10 + 3", High: "3d8 + 3" ),
            ( Low: "3d6 + 4", Medium: "3d8 + 4", High: "3d10 + 4" ),
            ( Low: "3d8 + 5", Medium: "3d10 + 5", High: "4d8 + 5" ),
            ( Low: "3d8 + 5", Medium: "4d8 + 5", High: "4d10 + 5" ),
            ( Low: "3d10 + 6", Medium: "4d8 + 6", High: "4d10 + 6" ),
            ( Low: "3d10 + 6", Medium: "4d10 + 7", High: "4d12 + 7" ),
            ( Low: "4d8 + 7", Medium: "4d10 + 7", High: "4d12 + 7" ),
            ( Low: "4d8 + 8", Medium: "4d12 + 8", High: "5d10 + 8" ),
            ( Low: "4d10 + 9", Medium: "5d10 + 9", High: "5d12 + 9" ),
            ( Low: "4d10 + 9", Medium: "5d10 + 9", High: "5d12 + 9" ),
        };

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

        public static int ExpectedProficiencyModifier(int level) => level / 2;
        public static int ExpectedAbilityModifier(int level, Index index) => StandardArrayBy4Levels[level / 4][index];
    }
}
