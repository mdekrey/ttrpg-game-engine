using GameEngine.Dice;
using GameEngine.RulesEngine;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace GameEngine.Rules
{
    public record DamageEffect(ImmutableList<DamageEffectEntry> DamageTypes) : IEffect;
    public record DamageEffectEntry(GameDiceExpression Amount, ImmutableList<DamageType> Types);

    public record HalfDamageEffect() : IEffect;
}
