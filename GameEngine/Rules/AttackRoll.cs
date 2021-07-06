using GameEngine.RulesEngine;
using System;

namespace GameEngine.Rules
{
    public class AttackRoll : IRandomDecisionMaker
    {
        private readonly ICurrentAttacker attacker;
        private readonly ICurrentTarget target;

        public enum AttackType
        {
            Physical,
        }

        public AttackRoll(ICurrentAttacker attacker, ICurrentTarget target)
        {
            this.attacker = attacker ?? throw new ArgumentNullException(nameof(attacker));
            this.target = target ?? throw new ArgumentNullException(nameof(target));
        }

        public Ability BaseAttackBonus { get; init; }
        public AttackType Type { get; init; } = AttackType.Physical;

        public int GetOutput(RandomGenerator random)
        {
            switch (Type)
            {
                case AttackType.Physical:
                    // TODO - get stats
                    return random(1, 21) - 10;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
