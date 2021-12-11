using GameEngine.Generator.Context;
using GameEngine.Generator.Text;
using System.Collections.Generic;

namespace GameEngine.Generator.Modifiers
{
    public abstract record AttackModifier() : IAttackModifier
    {
        public abstract int GetComplexity(PowerContext powerContext);
        public abstract PowerCost GetCost(AttackContext attackContext);
        public abstract IEnumerable<IAttackModifier> GetUpgrades(UpgradeStage stage, AttackContext attackContext);
        public virtual double ApplyEffectiveWeaponDice(double weaponDice) => weaponDice;

        public abstract AttackInfoMutator? GetAttackInfoMutator(AttackContext attackContext);
        public virtual bool CanUseRemainingPower() => false;
        public virtual ModifierFinalizer<IAttackModifier>? Finalize(AttackContext powerContext) => null;
    }
}
