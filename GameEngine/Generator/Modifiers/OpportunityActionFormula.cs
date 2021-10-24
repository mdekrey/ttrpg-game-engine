using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Generator.Text;
using GameEngine.Rules;

namespace GameEngine.Generator.Modifiers
{
    public record OpportunityActionFormula() : IPowerModifierFormula
    {
        public const string ModifierName = "OpportunityAction";

        public IEnumerable<IPowerModifier> GetBaseModifiers(UpgradeStage stage, PowerProfileBuilder power)
        {
            if (power.PowerInfo.Usage == PowerFrequency.AtWill)
                return Enumerable.Empty<IPowerModifier>();
            return
                stage != UpgradeStage.Standard ? Enumerable.Empty<IPowerModifier>() :
                new[] { new OpportunityActionModifier() };
        }

        public record OpportunityActionModifier() : PowerModifier(ModifierName)
        {
            public override int GetComplexity(PowerHighLevelInfo powerInfo) => 1;

            public override PowerCost GetCost(PowerProfileBuilder builder) => new PowerCost(PowerGenerator.GetBasePower(builder.PowerInfo.Level, builder.PowerInfo.Usage) - PowerGenerator.GetBasePower(builder.PowerInfo.Level, builder.PowerInfo.Usage - 1));

            public override IEnumerable<IPowerModifier> GetUpgrades(UpgradeStage stage, PowerProfileBuilder power) =>
                stage != UpgradeStage.Standard ? Enumerable.Empty<IPowerModifier>() :
                Enumerable.Empty<IPowerModifier>();

            public override PowerTextMutator? GetTextMutator(PowerProfile power) =>
                new(0, (textBlock, powerInfo) => textBlock with
                {
                    ActionType = "Immediate Reaction",
                    Trigger = "You or an ally is attacked by a creature", // TODO - other triggers?
                    Target = "The attacking creature",
                });
        }
    }
}
