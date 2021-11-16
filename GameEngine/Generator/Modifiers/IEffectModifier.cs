using GameEngine.Generator.Text;
using System.Collections.Generic;

namespace GameEngine.Generator.Modifiers
{
    public interface IEffectModifier : IModifier
    {
        PowerCost GetCost(TargetEffect builder, PowerProfileBuilder context);
        bool UsesDuration();
        bool IsInstantaneous();
        bool IsBeneficial();
        bool IsHarmful();
        IEnumerable<IEffectModifier> GetUpgrades(UpgradeStage stage, TargetEffect target, AttackProfile? attack, PowerProfileBuilder power);
        TargetInfoMutator? GetTargetInfoMutator(TargetEffect effect, PowerProfile power);
    }
}
