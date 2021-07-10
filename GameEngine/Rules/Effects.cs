using GameEngine.Dice;
using GameEngine.RulesEngine;
using System.Collections.Generic;
using System.Text;

namespace GameEngine.Rules
{
    public record DamageEffect(GameDiceExpression Damage, DamageType DamageType) : IEffect;
}
