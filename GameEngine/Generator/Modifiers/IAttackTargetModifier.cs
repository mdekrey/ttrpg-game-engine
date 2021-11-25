using GameEngine.Generator.Context;
using GameEngine.Generator.Text;
using System.Collections.Generic;

namespace GameEngine.Generator.Modifiers
{
    public interface IAttackTargetModifier : IModifier
    {
        Target GetTarget(AttackContext attackContext);
        PowerCost GetCost(AttackContext attackContext);
        IEnumerable<IAttackTargetModifier> GetUpgrades(UpgradeStage stage, AttackContext attackContext);

        string GetTargetText(AttackContext attackContext);
        AttackType GetAttackType(AttackContext attackContext);
        string? GetAttackNotes(AttackContext attackContext);
        TargetInfoMutator? GetTargetInfoMutator(AttackContext attackContext);

        IAttackTargetModifier Finalize(AttackContext powerContext);
    }
}
