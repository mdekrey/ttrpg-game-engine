using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Rules;
using static GameEngine.Generator.ImmutableConstructorExtension;

namespace GameEngine.Generator.Modifiers
{
    // TODO - merge this into opponent movement control
    public record MovementControlFormula() : AttackModifierFormula(ModifierName)
    {
        public const string ModifierName = "MovementControl";

        public override IAttackModifier GetBaseModifier(AttackProfileBuilder attack)
        {
            return new ConditionModifier(Build(new MovementControl("Prone")));
        }

        private static readonly ImmutableSortedDictionary<string, double> basicEffects =
            new[]
            {
                (Condition: "Prone", Cost: 1.0),
            }.ToImmutableSortedDictionary(e => e.Condition, e => e.Cost);

        public record ConditionModifier(ImmutableList<MovementControl> Effects) : AttackModifier(ModifierName)
        {
            public override int GetComplexity() => 1;
            public override PowerCost GetCost() => new PowerCost(Fixed: Effects.Select(c => c.Cost()).Sum());

            public override IEnumerable<RandomChances<IAttackModifier>> GetUpgrades(AttackProfileBuilder attack) =>
                from set in new[]
                {
                    from basicCondition in basicEffects.Keys
                    where !Effects.Select(b => b.Name).Contains(basicCondition)
                    select this with { Effects = Effects.Add(new MovementControl(basicCondition)) },

                    from condition in Effects
                    from upgrade in condition.GetUpgrades(attack.PowerInfo)
                    select this with { Effects = Effects.Remove(condition).Add(upgrade) },
                }
                from mod in set
                select new RandomChances<IAttackModifier>(mod);


            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attack)
            {
                // TODO
                return effect;
            }
        }

        public record MovementControl(string Name)
        {
            public virtual double Cost() => basicEffects[Name];
            public virtual IEnumerable<MovementControl> GetUpgrades(PowerHighLevelInfo powerInfo) =>
                Enumerable.Empty<MovementControl>();
        }

        public record ImmediateConditionModifier(string Name, PowerCost Cost) : AttackModifier(Name)
        {
            public override int GetComplexity() => 1;

            public override PowerCost GetCost() => Cost;

            public override IEnumerable<RandomChances<IAttackModifier>> GetUpgrades(AttackProfileBuilder attack) =>
                // TODO
                Enumerable.Empty<RandomChances<IAttackModifier>>();
            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile)
            {
                // TODO
                return effect;
            }
        }
    }

}
