using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Generator.Context;
using GameEngine.Generator.Text;
using GameEngine.Rules;

namespace GameEngine.Generator.Modifiers
{
    public record OpportunityActionFormula() : IPowerModifierFormula
    {
        public const string ModifierName = "OpportunityAction";

        public IEnumerable<IPowerModifier> GetBaseModifiers(UpgradeStage stage, PowerContext powerContext)
        {
            if (powerContext.Usage == PowerFrequency.AtWill)
                yield break;
            if (stage != UpgradeStage.Standard)
                yield break;
            if (powerContext.Modifiers.Any(m => m.ChangesActionType()))
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
            public override int GetComplexity(PowerContext powerContext) => 1;
            public override bool ChangesActionType() => true;

            public override PowerCost GetCost(PowerContext powerContext) => new PowerCost(
                PowerGenerator.GetBasePower(powerContext.Level, powerContext.Usage) - PowerGenerator.GetBasePower(powerContext.Level, powerContext.Usage - 1)
            );

            public override IEnumerable<IPowerModifier> GetUpgrades(UpgradeStage stage, PowerContext powerContext)
            {
                yield break;
            }

            public override PowerTextMutator? GetTextMutator(PowerContext powerContext) =>
                new(0, (textBlock) =>
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
