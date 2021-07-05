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
    }
}
