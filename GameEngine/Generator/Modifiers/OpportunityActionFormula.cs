using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Rules;

namespace GameEngine.Generator.Modifiers
{
    public record OpportunityActionFormula() : PowerModifierFormula(ModifierName)
    {
        public const string ModifierName = "OpportunityAction";

        public override bool IsValid(PowerProfileBuilder builder) => builder.PowerInfo.Usage != PowerFrequency.AtWill;
        public override IPowerModifier GetBaseModifier(PowerProfileBuilder power)
        {
            return new MaybeOpportunityActionModifier();
        }

        public record MaybeOpportunityActionModifier() : PowerModifier(ModifierName)
        {
            public override int GetComplexity() => 0;

            public override PowerCost GetCost(PowerProfileBuilder builder) => PowerCost.Empty;

            public override IEnumerable<IPowerModifier> GetPowerUpgrades(PowerProfileBuilder power, UpgradeStage stage) =>
                new[] { new OpportunityActionModifier() };

            public override PowerTextMutator? GetTextMutator() => throw new NotSupportedException("Should be upgraded or removed before this point");
        }

        public record OpportunityActionModifier() : PowerModifier(ModifierName)
        {
            public override int GetComplexity() => 0;

            public override PowerCost GetCost(PowerProfileBuilder builder) => new PowerCost(PowerGenerator.GetBasePower(builder.PowerInfo.Level, builder.PowerInfo.Usage) - PowerGenerator.GetBasePower(builder.PowerInfo.Level, builder.PowerInfo.Usage - 1));

            public override IEnumerable<IPowerModifier> GetPowerUpgrades(PowerProfileBuilder power, UpgradeStage stage) =>
                Enumerable.Empty<IPowerModifier>();

            public override PowerTextMutator? GetTextMutator() =>
                new(0, (textBlock, powerInfo) => textBlock with
                {
                    ActionType = "Immediate Reaction",
                    Trigger = "You or an ally is attacked by a creature", // TODO - other triggers?
                    Target = "The attacking creature",
                });
        }
    }
}
