using GameEngine.Generator.Context;
using GameEngine.Generator.Modifiers;
using GameEngine.Rules;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GameEngine.Generator
{
    public static class PowerProfileExtensions
    {
        public static IEnumerable<PowerProfile> PreApply(this IEnumerable<PowerProfile> powers, IBuildContext buildContext)
        {
            var options = from power in powers
                          let context = new PowerContext(power, buildContext.PowerInfo)
                          from builder in context.GetAttackContexts().Select(a => a.AttackContext).Aggregate(
                                    Enumerable.Repeat(ImmutableList<AttackProfile>.Empty, 1),
                                    (prev, next) => prev.SelectMany(l => next.PreApplyImplementNonArmorDefense(UpgradeStage.InitializeAttacks).Select(o => l.Add(o)))
                                )
                                .Select(attacks => power with { Attacks = attacks })
                          let b = builder.FullyInitialize(buildContext)
                          where b.AllModifiers(true).Any(p => p.CanUseRemainingPower()) // Ensures ABIL mod or multiple hits
                          select b;

            return options;
        }

        public static PowerProfile FullyInitialize(this PowerProfile builder, IBuildContext buildContext)
        {
            while (builder.GetUpgrades(buildContext.PowerInfo, UpgradeStage.InitializeAttacks).Where(buildContext.IsValid).FirstOrDefault() is PowerProfile next)
                builder = next;
            return builder;
        }

        // Implements get free non-armor defense due to lack of proficiency bonus
        private static IEnumerable<AttackProfile> PreApplyImplementNonArmorDefense(this AttackContext attack, UpgradeStage stage) =>
            attack.ToolType is ToolType.Implement ? ModifierDefinitions.NonArmorDefense.Apply(attack, stage) : new[] { attack.Attack };

        private static IEnumerable<AttackProfile> Apply(this IAttackModifierFormula formula, AttackContext attackContext, UpgradeStage stage)
        {
            return from mod in formula.GetBaseModifiers(stage, attackContext)
                   where !attackContext.Modifiers.Any(m => m.GetName() == mod.GetName())
                   select attackContext.Attack.Apply(mod);
        }

        public static string ToKeyword(this ToolType tool) =>
            tool switch
            {
                ToolType.Implement => "Implement",
                ToolType.Weapon => "Weapon",
                _ => throw new ArgumentException("Invalid enum value for tool", nameof(tool)),
            };

    }
}
