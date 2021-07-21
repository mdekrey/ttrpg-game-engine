using GameEngine.Dice;
using GameEngine.RulesEngine;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace GameEngine.Rules
{
    public record DamageEffect(ImmutableDictionary<DamageType, GameDiceExpression> DamageTypes) : IEffect;
}
