using System.Collections.Generic;
using System.Linq;
using GameEngine.Generator.Text;
using GameEngine.Rules;

namespace GameEngine.Generator.Modifiers
{
    public record MinorActionFormula() : IPowerModifierFormula
    {
        public IEnumerable<IPowerModifier> GetBaseModifiers(UpgradeStage stage, PowerProfileBuilder power)
        {
            if (power.PowerInfo.Usage == PowerFrequency.AtWill)
                yield break;
            if (stage != UpgradeStage.Standard)
                yield break;
            if (power.Modifiers.Any(m => m.ChangesActionType()))
                yield break;

            yield return new MinorActionModifier();
        }

        public record MinorActionModifier() : PowerModifier("Minor Action")
        {
            public override int GetComplexity(PowerHighLevelInfo powerInfo) => 1;
            public override bool ChangesActionType() => true;

            public override PowerCost GetCost(PowerProfileBuilder builder) => new PowerCost(Fixed: 1.5);

            public override IEnumerable<IPowerModifier> GetUpgrades(UpgradeStage stage, PowerProfileBuilder power)
            {
                yield break;
            }

            public override PowerTextMutator? GetTextMutator(PowerProfile power) =>
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
