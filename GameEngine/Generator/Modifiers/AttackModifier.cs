using GameEngine.Generator.Text;
using System.Collections.Generic;

namespace GameEngine.Generator.Modifiers
{
    public abstract record AttackModifier(string Name) : IAttackModifier
    {
        public abstract int GetComplexity(PowerHighLevelInfo powerInfo);
        public abstract PowerCost GetCost(AttackProfile builder, PowerProfileBuilder power);
        public virtual bool IsPlaceholder() => false;
        public abstract IEnumerable<IAttackModifier> GetUpgrades(UpgradeStage stage, AttackProfile attack, PowerProfileBuilder power);
        public virtual double ApplyEffectiveWeaponDice(double weaponDice) => weaponDice;

        public abstract AttackInfoMutator? GetAttackInfoMutator(PowerProfile power);
        public virtual bool CanUseRemainingPower() => false;
    }
}
