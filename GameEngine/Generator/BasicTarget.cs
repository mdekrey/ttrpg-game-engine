using GameEngine.Generator.Modifiers;
using GameEngine.Generator.Context;
using GameEngine.Generator.Text;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameEngine.Generator
{
    public record BasicTarget(Target Target) : IEffectTargetModifier, IAttackTargetModifier
    {
        public string Name => "Basic Target";
        public int GetComplexity(PowerContext powerContext) => 0;

        public PowerCost GetCost() => PowerCost.Empty;

        public Target GetTarget() => Target;
        public TargetInfoMutator? GetTargetInfoMutator() => null;
        public string? GetAttackNotes() => null;

        public IEnumerable<IEffectTargetModifier> GetUpgrades(UpgradeStage stage, EffectContext effectContext)
        {
            return from formula in ModifierDefinitions.advancedTargetModifiers
                   from mod in formula.GetBaseModifiers(stage, effectContext)
                   select mod;
        }

        public IEnumerable<IAttackTargetModifier> GetUpgrades(UpgradeStage stage, AttackContext attackContext)
        {
            return from formula in ModifierDefinitions.advancedTargetModifiers
                   from mod in formula.GetBaseModifiers(stage, attackContext)
                   select mod;
        }


        public string GetTargetText()
        {
            return Target switch
            {
                Target.Enemy => "one enemy",
                Target.Self => "you",
                Target.Self | Target.Enemy => "you or one enemy", // This may be a good one for "If you take damage from this power, deal damage to all enemies instead." or something
                Target.Ally => "one of your allies",
                Target.Ally | Target.Enemy => "one creature other than yourself",
                Target.Ally | Target.Self => "you or one of your allies",
                Target.Ally | Target.Self | Target.Enemy => "one creature",

                _ => throw new NotSupportedException(),
            };
        }

        public AttackType GetAttackType(PowerProfile power)
        {
            return (power.Tool, power.ToolRange) switch
            {
                (ToolType.Weapon, ToolRange.Melee) => new MeleeWeaponAttackType(),
                (ToolType.Implement, ToolRange.Melee) => new MeleeTouchAttackType(),
                (ToolType.Weapon, ToolRange.Range) => new RangedWeaponAttackType(),
                (ToolType.Implement, ToolRange.Range) => new RangedAttackType(10),
                _ => throw new NotSupportedException(),
            };
        }

        Target IEffectTargetModifier.GetTarget(EffectContext effectContext) => GetTarget();

        PowerCost IEffectTargetModifier.GetCost(EffectContext effectContext) => GetCost();

        TargetInfoMutator? IEffectTargetModifier.GetTargetInfoMutator(EffectContext effectContext) => GetTargetInfoMutator();

        Target IAttackTargetModifier.GetTarget(AttackContext attackContext) => GetTarget();

        PowerCost IAttackTargetModifier.GetCost(AttackContext attackContext) => GetCost();

        string IAttackTargetModifier.GetTargetText(AttackContext attackContext) => GetTargetText();

        AttackType IAttackTargetModifier.GetAttackType(AttackContext attackContext) => GetAttackType(attackContext.PowerProfile);

        string? IAttackTargetModifier.GetAttackNotes(AttackContext attackContext) => GetAttackNotes();

        TargetInfoMutator? IAttackTargetModifier.GetTargetInfoMutator(AttackContext attackContext) => GetTargetInfoMutator();

        string IEffectTargetModifier.GetTargetText(EffectContext effectContext) => GetTargetText();

        AttackType IEffectTargetModifier.GetAttackType(EffectContext effectContext) => GetAttackType(effectContext.PowerProfile);

        string? IEffectTargetModifier.GetAttackNotes(EffectContext effectContext) => GetAttackNotes();
    }
}