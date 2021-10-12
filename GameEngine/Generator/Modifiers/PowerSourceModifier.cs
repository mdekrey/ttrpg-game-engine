using System;
using System.Collections.Generic;

namespace GameEngine.Generator.Modifiers
{
    public record PowerSourceModifier(string PowerSource): PowerModifier(ModifierName)
    {
        const string ModifierName = "Power Source";

        public override bool ExcludeFromUniqueness() => true;

        public override int GetComplexity(PowerHighLevelInfo powerInfo) => 0;

        public override PowerCost GetCost(PowerProfileBuilder builder) => PowerCost.Empty;

        public override IEnumerable<IPowerModifier> GetPowerUpgrades(PowerProfileBuilder power, UpgradeStage stage)
        {
            yield break;
        }

        public override PowerTextMutator? GetTextMutator(PowerProfile power)
        {
            return new(0, (pt, pi) => pt with { Keywords = pt.Keywords.Items.Add(PowerSource) });
        }
    }

}
