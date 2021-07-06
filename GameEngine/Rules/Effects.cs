using GameEngine.Dice;
using GameEngine.RulesEngine;
using System.Collections.Generic;
using System.Text;

namespace GameEngine.Rules
{
    public record DieCodeRandomDecisionMaker(DieCodes Dice) : IRandomDecisionMaker
    {
        public int GetOutput(RandomGenerator random) => Dice.Roll(random);
    }

    public record DamageEffect(DieCodes Damage, DamageType DamageType) : IEffect;
}
