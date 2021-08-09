using System.Collections.Generic;
using System.Collections.Immutable;
using GameEngine.Rules;
using System.Linq;
using static GameEngine.Generator.ImmutableConstructorExtension;

namespace GameEngine.Generator.Modifiers
{
    public record ConditionFormula(ImmutableList<string> Keywords, string Name, ImmutableDictionary<Duration, PowerCost> PowerCost) : PowerModifierFormula(Keywords, Name)
    {
        public ConditionFormula(ImmutableList<string> Keywords, string Name)
            : this(Keywords, Name, Build((Duration.SaveEnds, new PowerCost(Fixed: 1)), (Duration.EndOfUserNextTurn, new PowerCost(Fixed: 0.5))))
        {
        }

        public override IEnumerable<ApplicablePowerModifierFormula> GetOptions(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo)
        {
            if (HasModifier(attack)) yield break;
            var prevThreshold = 0;
            foreach (var powerCost in PowerCost.EscalatingOdds())
            {
                yield return new(BuildModifier(powerCost.result.Key, powerCost.result.Value), Chances: powerCost.threshold - prevThreshold);
                prevThreshold = powerCost.threshold;
            }

            PowerModifierBuilder BuildModifier(Duration duration, PowerCost cost) =>
                new (Name, cost, Build(
                    ("Duration", duration.ToString("g"))
                ));
        }

        public IEnumerable<(PowerCost cost, Duration duration)> GetAvailableOptions(AttackProfileBuilder attack)
        {
            return from kvp in PowerCost
                   orderby kvp.Key descending
                   where attack.CanApply(kvp.Value)
                   select (cost: kvp.Value, duration: kvp.Key);
        }

        public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile, PowerModifier modifier)
        {
            // TODO
            return effect;
        }
    }
}
