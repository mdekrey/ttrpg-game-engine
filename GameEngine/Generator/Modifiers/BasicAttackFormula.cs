using System;
using System.Collections.Generic;
using GameEngine.Generator.Context;
using GameEngine.Generator.Text;
using GameEngine.Rules;

namespace GameEngine.Generator.Modifiers
{
    public record BasicAttackFormula() : IPowerModifierFormula
    {
        public IEnumerable<IPowerModifier> GetBaseModifiers(UpgradeStage stage, PowerContext powerContext)
        {
            if (powerContext.Usage != PowerFrequency.AtWill)
                yield break; 
            if (stage != UpgradeStage.Standard)
                yield break;
            if (powerContext.ToolType != ToolType.Implement)
                yield break;
            if (powerContext.Attacks.Count == 0 || powerContext.Attacks[0].Effects.Count == 0 || powerContext.Attacks[0].Effects[0].EffectType != EffectType.Harmful)
                yield break;
            yield return new IsBasicAttackModifier();
        }

        public record IsBasicAttackModifier() : PowerModifier("Is Basic Attack")
        {
            public override int GetComplexity(PowerContext powerContext) => 1;

            public override PowerCost GetCost(PowerContext powerContext) =>
                new PowerCost(Fixed: 0.5);

            public override PowerTextMutator? GetTextMutator(PowerContext powerContext) =>
                new(10000, (textBlock, flavor) =>
                {
                    var meleeOrRanged = powerContext.ToolRange switch
                    {
                        ToolRange.Melee => "melee",
                        ToolRange.Range => "ranged",
                        _ => throw new NotImplementedException(),
                    };
                    return (textBlock with
                    {
                        RulesText = textBlock.RulesText.Items.Add(new(
                            "Special", $"This power counts as a {meleeOrRanged} basic attack. When a power allows you to make a {meleeOrRanged} basic attack, you can use this power."
                        ))
                    }, flavor);
                });

            public override IEnumerable<IPowerModifier> GetUpgrades(UpgradeStage stage, PowerContext powerContext)
            {
                yield break;
            }
        }
    }
}
