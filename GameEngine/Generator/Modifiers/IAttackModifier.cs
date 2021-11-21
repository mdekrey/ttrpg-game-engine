using GameEngine.Generator.Context;
using GameEngine.Generator.Text;
using System.Collections.Generic;

namespace GameEngine.Generator.Modifiers
{
    public interface IAttackModifier : IModifier
    {
        PowerCost GetCost(AttackContext attackContext);
        double ApplyEffectiveWeaponDice(double weaponDice);
        AttackInfoMutator? GetAttackInfoMutator(AttackContext attackContext);
        IEnumerable<IAttackModifier> GetUpgrades(UpgradeStage stage, AttackContext attackContext);
    }
}
