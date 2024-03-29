﻿using System.Collections.Generic;
using GameEngine.Combining;
using GameEngine.Generator.Context;
using GameEngine.Generator.Text;
using GameEngine.Rules;

namespace GameEngine.Generator.Modifiers
{
    public record MakeBasicAttackFormula() : IEffectFormula
    {
        public IEnumerable<IEffectModifier> GetBaseModifiers(UpgradeStage stage, EffectContext effectContext)
        {
            if (stage != UpgradeStage.Standard)
                yield break;
            if (effectContext.EffectType != EffectType.Beneficial)
                yield break;

            yield return new MakeBasicAttackModifier(GameDiceExpression.Empty);
        }

        [ModifierName("Make Basic Attack")]
        public record MakeBasicAttackModifier(GameDiceExpression Damage) : EffectModifier()
        {
            public override int GetComplexity(PowerContext powerContext) => 1;
            public override bool UsesDuration() => false;
            public override bool IsInstantaneous() => true;
            public override bool IsBeneficial() => true;
            public override bool IsHarmful() => false;

            // Even though a basic attack is 1.5, 4e uses 1 to encourage giving your rolls to other players
            public override PowerCost GetCost(EffectContext effectContext) => new (Fixed: 1 + Damage.ToWeaponDice());

            public override IEnumerable<IEffectModifier> GetUpgrades(UpgradeStage stage, EffectContext effectContext)
            {
                if (Damage == GameDiceExpression.Empty)
                    foreach (var ability in effectContext.Abilities)
                        yield return this with { Damage = Damage + ability };
            }

            public override TargetInfoMutator? GetTargetInfoMutator(EffectContext effectContext, bool half) =>
                half ? null :
                new(2000, (target) => target with
                {
                    Parts = Damage  == GameDiceExpression.Empty ? target.Parts.Add("may immediately make a basic attack as a free action")
                        : target.Parts.Add($"may immediately make a basic attack as a free action and add {Damage} to the damage."),
                });

            public override CombineResult<IEffectModifier> Combine(IEffectModifier mod)
            {
                if (mod is not MakeBasicAttackModifier other)
                    return CombineResult<IEffectModifier>.Cannot;
                return CombineResult<IEffectModifier>.Use(Damage.ToWeaponDice() >= other.Damage.ToWeaponDice() ? this : other);
            }
        }
    }
}
