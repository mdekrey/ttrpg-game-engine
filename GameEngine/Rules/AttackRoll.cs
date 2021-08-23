using GameEngine.RulesEngine;
using System;
using System.Linq;

namespace GameEngine.Rules
{
    public class AttackRoll : IRandomizedEffect
    {
        public Ability Kind { get; init; }
        public GameDiceExpression Bonus { get; init; } = GameDiceExpression.Empty;
        public DefenseType Defense { get; init; } = DefenseType.ArmorClass;
        public IEffect? Hit { get; init; }
        public IEffect? Miss { get; init; }
        public IEffect? Effect { get; init; }

        public int GetOutput(RandomGenerator random)
        {
            switch (Defense)
            {
                case DefenseType.ArmorClass:
                    // TODO - get stats
                    return random(1, 21) - 10;
                default:
                    throw new NotImplementedException();
            }
        }

        public IEffect GetEffect(RandomGenerator random)
        {
            return AllEffects.FromMulti(new[]
            {
                GetOutput(random) >= 0 ? Hit : Miss,
                Effect
            }.Where(e => e != null)!);
        }
    }
}
