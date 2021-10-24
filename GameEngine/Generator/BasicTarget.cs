using GameEngine.Generator.Modifiers;
using GameEngine.Generator.Text;
using System.Collections.Generic;

namespace GameEngine.Generator
{
    public record BasicTarget(Target Target) : ITargetModifier
    {
        public string Name => "Basic Target";

        public int GetComplexity(PowerHighLevelInfo powerInfo) => 0;

        public PowerCost GetCost(TargetEffectBuilder builder, PowerProfileBuilder context) => PowerCost.Empty;

        public Target GetTarget() => Target;

        public TargetInfoMutator? GetTargetInfoMutator(TargetEffect effect, PowerProfile power)
        {
            // TODO
            throw new System.NotImplementedException();
        }

        public IEnumerable<ITargetModifier> GetUpgrades(UpgradeStage stage, TargetEffectBuilder target, PowerProfileBuilder power, int targetEffectIndex)
        {
            yield break;
        }

        public bool IsBasic() => true;
    }
}