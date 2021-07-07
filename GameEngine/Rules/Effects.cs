using GameEngine.Dice;
using GameEngine.RulesEngine;
using System.Collections.Generic;
using System.Text;

namespace GameEngine.Rules
{
    public record DamageEffect(DieCodes Damage, DamageType DamageType) : IEffect;
    public record WeaponDamageEffect() : IEffect;
}
