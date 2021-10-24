using GameEngine.Generator.Text;
using System.Collections.Generic;

namespace GameEngine.Generator.Modifiers
{
    public interface IEffectModifier : IModifier
    {
        PowerCost GetCost(TargetEffectBuilder builder, PowerProfileBuilder context);
        Target ValidTargets();
        bool UsesDuration();
        bool EnablesSaveEnd();
        IEnumerable<IEffectModifier> GetUpgrades(UpgradeStage stage, TargetEffectBuilder target, PowerProfileBuilder power);
        TargetInfoMutator? GetTargetInfoMutator(TargetEffect effect, PowerProfile power);
    }
    public interface ITargetModifier : IModifier
    {
        PowerCost GetCost(TargetEffectBuilder builder, PowerProfileBuilder context);
        Target ValidTargets();
        IEnumerable<ITargetModifier> GetUpgrades(UpgradeStage stage, TargetEffectBuilder target, PowerProfileBuilder power);
        TargetInfoMutator? GetTargetInfoMutator(TargetEffect effect, PowerProfile power);
    }
}
