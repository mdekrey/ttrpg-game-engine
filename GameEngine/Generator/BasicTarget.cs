using GameEngine.Generator.Modifiers;
using GameEngine.Generator.Text;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameEngine.Generator
{
    public record BasicTarget(Target Target) : ITargetModifier
    {
        public string Name => "Basic Target";

        public int GetComplexity(PowerHighLevelInfo powerInfo) => 0;

        public PowerCost GetCost(TargetEffectBuilder builder, PowerProfileBuilder context) => PowerCost.Empty;

        public Target GetTarget() => Target;

        public string GetTargetText(PowerProfile power, int? attackIndex)
        {
            return Target switch
            {
                Target.Enemy => "One enemy",
                Target.Self => "You",
                Target.Self | Target.Enemy => "You or one enemy", // This may be a good one for "If you take damage from this power, deal damage to all enemies instead." or something
                Target.Ally => "One of your allies",
                Target.Ally | Target.Enemy => "One creature other than yourself",
                Target.Ally | Target.Self => "You or one of your allies",
                Target.Ally | Target.Self | Target.Enemy => "One creature",

                _ => throw new NotSupportedException(),
            };
        }
    
        public IEnumerable<ITargetModifier> GetUpgrades(UpgradeStage stage, TargetEffectBuilder target, PowerProfileBuilder power, int? attackIndex)
        {
            return from formula in ModifierDefinitions.advancedTargetModifiers
                   from mod in formula.GetBaseModifiers(stage, target, power, attackIndex)
                   select mod;
        }
    }
}