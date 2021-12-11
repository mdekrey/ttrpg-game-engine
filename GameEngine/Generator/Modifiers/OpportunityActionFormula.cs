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
        public IEnumerable<IPowerModifier> GetBaseModifiers(UpgradeStage stage, PowerContext powerContext)
        {
            if (powerContext.Usage == PowerFrequency.AtWill)
                yield break;
            if (stage != UpgradeStage.Standard)
                yield break;
            if (powerContext.Modifiers.Any(m => m.ChangesActionType()))
                yield break;

            yield return new OpportunityActionModifier(Trigger.Custom);
            yield return new OpportunityActionModifier(Trigger.YouOrAllyAttacked);
            yield return new OpportunityActionModifier(Trigger.ACreatureMovesAdjacent);
        }

        public enum Trigger
        {
            Custom,
            YouOrAllyAttacked,
            ACreatureMovesAdjacent,
        }

        [ModifierName("OpportunityAction")]
        public record OpportunityActionModifier(Trigger Trigger) : PowerModifier()
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
                new(0, (textBlock, flavor) =>
                {
                    var result = textBlock with
                    {
                        ActionType = "Immediate Reaction",
                    };

                    switch (Trigger)
                    {
                        case Trigger.Custom:
                            var trigger = flavor.GetText("Trigger", "An adjacent enemy marked by you moves without shifting on its turn", out flavor);
                            var target = flavor.GetText("Target", "The triggering enemy", out flavor);
                            return (result with
                            {
                                Trigger = trigger,
                                Target = target,
                            }, flavor);

                        case Trigger.YouOrAllyAttacked:
                            return (result with
                            {
                                Trigger = "You or an ally is attacked by a creature",
                                Target = "The attacking creature",
                            }, flavor);

                        case Trigger.ACreatureMovesAdjacent:
                            return (result with
                            {
                                Trigger = "A creature moves adjacent to you",
                                Target = "The moving creature",
                            }, flavor);

                        default:
                            throw new NotSupportedException();
                    }
                });
        }
    }
}
