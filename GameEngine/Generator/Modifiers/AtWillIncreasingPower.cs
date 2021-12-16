using GameEngine.Generator.Context;
using GameEngine.Generator.Text;
using GameEngine.Rules;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameEngine.Generator.Modifiers
{
    [ModifierName("Increase Damage by Level")]
    public record AtWillIncreasingPower() : PowerModifier()
    {
        public override int GetComplexity(PowerContext powerContext) => 0;

        public override PowerCost GetCost(PowerContext powerContext) => PowerCost.Empty;

        public override IEnumerable<PowerProfile> TrySimplifySelf(PowerProfile builder)
        {
            return base.TrySimplifySelf(builder);
        }

        public override PowerTextMutator? GetTextMutator(PowerContext powerContext)
        {
            var damageModifiers = powerContext.PowerProfile.GetDamageLenses().Select(d => d.Damage).Where(d => d.Damage.DieCodes != Dice.DieCodes.Empty || d.Damage.WeaponDiceCount != 0).ToArray();
            if (!damageModifiers.Any())
                return null;
            var increases = from damage in damageModifiers.Distinct()
                            let initial = damage.Damage
                            let increased = initial with
                            {
                                DieCodes = initial.DieCodes * 2,
                                WeaponDiceCount = initial.WeaponDiceCount * 2,
                            }
                            let initialText = (damage with { Damage = initial }).DamageText()
                            let increasedText = (damage with { Damage = increased }).DamageText()
                            select $"from {initialText} to {increasedText}";
            return new PowerTextMutator(-1000, (textBlock, flavor) =>
            {
                return (textBlock with
                {
                    RulesText = textBlock.RulesText.Items.Add(new RulesText("Level 21", "Increase " + ProseHelpers.OxfordComma(increases.ToArray())))
                }, flavor);
            });
        }

        public override IEnumerable<IPowerModifier> GetUpgrades(UpgradeStage stage, PowerContext powerContext)
        {
            yield break;
        }
    }
}
