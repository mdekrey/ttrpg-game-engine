﻿using System.Collections.Generic;
using System.Text;
using System.Collections.Immutable;
using GameEngine.Rules;
using System.Linq;
using static GameEngine.Generator.ImmutableConstructorExtension;
using static GameEngine.Generator.PowerBuildingExtensions;
using GameEngine.Generator.Text;

namespace GameEngine.Generator.Modifiers
{

    public record ToHitBonusFormula() : IAttackModifierFormula
    {
        public const string ModifierName = "To-Hit Bonus to Current Attack";

        public IEnumerable<IAttackModifier> GetBaseModifiers(UpgradeStage stage, AttackProfile attack, PowerProfileBuilder power)
        {
            return new ToHitBonus(0).GetUpgrades(stage, attack, power);
        }

        public record ToHitBonus(GameDiceExpression Amount) : AttackModifier(ModifierName)
        {
            public override int GetComplexity(PowerHighLevelInfo powerInfo) => IsPlaceholder() ? 0 : 1;
            public override bool IsPlaceholder() => Amount == GameDiceExpression.Empty;

            public override PowerCost GetCost(AttackProfile builder, PowerProfileBuilder power) => new PowerCost(Amount.ToWeaponDice());

            public override IEnumerable<IAttackModifier> GetUpgrades(UpgradeStage stage, AttackProfile attack, PowerProfileBuilder power)
            {
                if (stage < UpgradeStage.Standard) yield break;

                if (Amount.Abilities == CharacterAbilities.Empty)
                {
                    if (Amount.DieCodes.Modifier < 8) // actually 10
                        yield return this with { Amount = Amount.StepUpModifier() };
                    if (Amount.DieCodes.Modifier <= 2)
                    {
                        foreach (var ability in power.PowerInfo.ToolProfile.Abilities.Where(a => a != attack.Ability))
                            yield return this with { Amount = Amount + ability };
                    }
                }
                else if (Amount.DieCodes.Modifier == 0)
                    yield return this with { Amount = 0 };
            }

            public override AttackInfoMutator? GetAttackInfoMutator(PowerProfile power) =>
                new(0, (attack, index) => attack with { AttackExpression = attack.AttackExpression + Amount });
        }
    }

}
