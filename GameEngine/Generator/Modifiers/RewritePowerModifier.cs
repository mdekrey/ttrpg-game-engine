using GameEngine.Generator.Context;
using GameEngine.Generator.Text;
using System.Collections.Generic;

namespace GameEngine.Generator.Modifiers
{
    public abstract record RewritePowerModifier() : PowerModifier("N/A" /* This doesn't matter because it'll be rewritten */)
    {
        public override int GetComplexity(PowerContext powerContext) => 0;

        public override PowerCost GetCost(PowerContext powerContext) =>
            PowerCost.Empty;

        public override IEnumerable<IPowerModifier> GetUpgrades(UpgradeStage stage, PowerContext powerContext) =>
            System.Linq.Enumerable.Empty<IPowerModifier>();

        public override PowerTextMutator? GetTextMutator(PowerContext powerContext) => throw new System.NotSupportedException("Should be upgraded or removed before this point");

        public override abstract IEnumerable<PowerProfile> TrySimplifySelf(PowerProfile builder);
    }
}
