using GameEngine.Generator.Context;
using GameEngine.Generator.Text;
using System.Collections.Generic;

namespace GameEngine.Generator.Modifiers
{
    public interface IEffectTargetModifier : IModifier
    {
        Target GetTarget(EffectContext effectContext);
        PowerCost GetCost(EffectContext effectContext);
        IEnumerable<IEffectTargetModifier> GetUpgrades(UpgradeStage stage, EffectContext effectContext);

        string GetTargetText(EffectContext effectContext);
        AttackType GetAttackType(EffectContext effectContext);
        string? GetAttackNotes(EffectContext effectContext);
        TargetInfoMutator? GetTargetInfoMutator(EffectContext effectContext);

        IEffectTargetModifier Finalize(EffectContext powerContext);
    }
}
