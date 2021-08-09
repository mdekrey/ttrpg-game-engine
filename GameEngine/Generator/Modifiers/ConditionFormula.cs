using System.Collections.Generic;
using System.Collections.Immutable;
using GameEngine.Rules;
using System.Linq;
using static GameEngine.Generator.ImmutableConstructorExtension;

namespace GameEngine.Generator.Modifiers
{
    public record ConditionFormula(ImmutableList<string> Keywords) : PowerModifierFormula(Keywords, ModifierName)
    {
        public const string ModifierName = "Condition";

        private static readonly ImmutableDictionary<(string ConditionName, Duration Duration), PowerCost> conditions = ImmutableDictionary<(string ConditionName, Duration duration), PowerCost>.Empty
            .Add(("Slowed", Duration.EndOfUserNextTurn), new PowerCost(0.5))
            .Add(("Slowed", Duration.SaveEnds), new PowerCost(1))
            .Add(("Dazed", Duration.EndOfUserNextTurn), new PowerCost(0.5))
            .Add(("Dazed", Duration.SaveEnds), new PowerCost(1))
            .Add(("Immobilized", Duration.EndOfUserNextTurn), new PowerCost(1))
            .Add(("Immobilized", Duration.SaveEnds), new PowerCost(2))
            .Add(("Weakened", Duration.EndOfUserNextTurn), new PowerCost(1))
            .Add(("Weakened", Duration.SaveEnds), new PowerCost(2))
            .Add(("Grants Combat Advantage", Duration.EndOfUserNextTurn), new PowerCost(1))
            .Add(("Grants Combat Advantage", Duration.SaveEnds), new PowerCost(2))
            .Add(("Blinded", Duration.EndOfUserNextTurn), new PowerCost(1))
            .Add(("Blinded", Duration.SaveEnds), new PowerCost(2));

        public override IEnumerable<RandomChances<PowerModifier>> GetOptions(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo)
        {
            if (HasModifier(attack)) yield break;
            
            foreach (var entry in conditions)
            {
                var chances = 5 - (int)(entry.Value.Fixed / 0.5);

                yield return new(new ConditionModifier(entry.Key.Duration, Build(entry.Key.ConditionName)), Chances: chances);
            }
        }

        public record ConditionModifier(Duration Duration, ImmutableList<string> Conditions) : PowerModifier(ModifierName)
        {
            public override int GetComplexity() => 1;
            public override PowerCost GetCost() => Conditions.Select(c => conditions[(c, Duration)]).Aggregate((a, b) => a + b);

            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile)
            {
                // TODO
                return effect;
            }

        }
    }
}
