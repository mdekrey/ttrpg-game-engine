using System;
using System.Collections.Generic;
using GameEngine.Generator.Text;
using GameEngine.Rules;

namespace GameEngine.Generator.Modifiers
{
    public record BasicAttackFormula() : IPowerModifierFormula
    {
        public IEnumerable<IPowerModifier> GetBaseModifiers(UpgradeStage stage, PowerProfileBuilder power)
        {
            if (power.PowerInfo.Usage != PowerFrequency.AtWill)
                yield break; 
            if (stage != UpgradeStage.Standard)
                yield break;
            if (power.PowerInfo.ToolProfile.Type != ToolType.Implement)
                yield break;
            if (power.Attacks.Count == 0 || power.Attacks[0].Effects.Count == 0 || power.Attacks[0].Effects[0].EffectType != EffectType.Harmful)
                yield break;
            yield return new IsBasicAttackModifier();
        }

        public record IsBasicAttackModifier() : PowerModifier("Is Basic Attack")
        {
            public override int GetComplexity(PowerHighLevelInfo powerInfo) => 1;

            public override PowerCost GetCost(PowerProfileBuilder builder) =>
                new PowerCost(Fixed: 0.5);

            public override PowerTextMutator? GetTextMutator(PowerProfile power) =>
                new(10000, (textBlock, powerInfo) =>
                {
                    var meleeOrRanged = powerInfo.ToolRange switch
                    {
                        ToolRange.Melee => "melee",
                        ToolRange.Range => "ranged",
                        _ => throw new NotImplementedException(),
                    };
                    return textBlock with
                    {
                        RulesText = textBlock.RulesText.Items.Add(new(
                            "Special", $"This power counts as a {meleeOrRanged} basic attack. When a power allows you to make a {meleeOrRanged} basic attack, you can use this power."
                        ))
                    };
                });

            public override IEnumerable<IPowerModifier> GetUpgrades(UpgradeStage stage, PowerProfileBuilder power)
            {
                yield break;
            }
        }
    }
}
