using System.Collections.Generic;
using System.Linq;
using GameEngine.Generator.Context;
using GameEngine.Generator.Text;
using GameEngine.Rules;

namespace GameEngine.Generator.Modifiers
{
    public record MinorActionFormula() : IPowerModifierFormula
    {
        public IEnumerable<IPowerModifier> GetBaseModifiers(UpgradeStage stage, PowerContext powerContext)
        {
            if (powerContext.Usage == PowerFrequency.AtWill)
                yield break;
            if (stage != UpgradeStage.Standard)
                yield break;
            if (powerContext.Modifiers.Any(m => m.ChangesActionType()))
                yield break;

            yield return new MinorActionModifier();
        }

        public record MinorActionModifier() : PowerModifier("Minor Action")
        {
            public override int GetComplexity(PowerContext powerContext) => 1;
            public override bool ChangesActionType() => true;

            public override PowerCost GetCost(PowerContext powerContext) => new PowerCost(Fixed: 1.5);

            public override IEnumerable<IPowerModifier> GetUpgrades(UpgradeStage stage, PowerContext powerContext)
            {
                yield break;
            }

            public override PowerTextMutator? GetTextMutator(PowerContext powerContext) =>
                new(0, (textBlock, powerInfo) =>
                {
                    return textBlock with
                    {
                        ActionType = "Minor Action",
                    };
                });
        }
    }
}
