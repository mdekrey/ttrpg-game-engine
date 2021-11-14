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
                yield break;
            if (stage != UpgradeStage.Standard)
                yield break;
            if (power.Modifiers.Any(m => m.ChangesActionType()))
                yield break;

            yield return new OpportunityActionModifier(Trigger.YouOrAllyAttacked);
            yield return new OpportunityActionModifier(Trigger.ACreatureMovesAdjacent);
        }

        public enum Trigger
        {
            YouOrAllyAttacked,
            ACreatureMovesAdjacent,
            // TODO - more triggers
        }

        public record OpportunityActionModifier(Trigger Trigger) : PowerModifier(ModifierName)
        {
            public override int GetComplexity(PowerHighLevelInfo powerInfo) => 1;
            public override bool ChangesActionType() => true;

            public override PowerCost GetCost(PowerProfileBuilder builder) => new PowerCost(PowerGenerator.GetBasePower(builder.PowerInfo.Level, builder.PowerInfo.Usage) - PowerGenerator.GetBasePower(builder.PowerInfo.Level, builder.PowerInfo.Usage - 1));

            public override IEnumerable<IPowerModifier> GetUpgrades(UpgradeStage stage, PowerProfileBuilder power)
            {
                yield break;
            }

            public override PowerTextMutator? GetTextMutator(PowerProfile power) =>
                new(0, (textBlock, powerInfo) =>
                {
                    var result = textBlock with
                    {
                        ActionType = "Immediate Reaction",
                    };
                    return Trigger switch
                    {
                        Trigger.YouOrAllyAttacked => result with
                        {
                            Trigger = "You or an ally is attacked by a creature",
                            Target = "The attacking creature",
                        },
                        Trigger.ACreatureMovesAdjacent => result with
                        {
                            Trigger = "A creature moves adjacent to you",
                            Target = "The moving creature",
                        },
                        _ => throw new NotImplementedException(),
                    };
                });
        }
    }
}
