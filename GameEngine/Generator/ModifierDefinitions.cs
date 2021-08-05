using System;
using System.Collections.Generic;
using System.Text;
using static GameEngine.Generator.PowerDefinitions;
using System.Collections.Immutable;
using GameEngine.Rules;
using System.Linq;
using static GameEngine.Generator.ImmutableConstructorExtension;

namespace GameEngine.Generator
{
    public static class ModifierDefinitions
    {
        public const string GeneralKeyword = "General";

        public static readonly PowerModifierFormula NonArmorDefense = new NonArmorDefenseFormula(AccuratePowerTemplateName);
        public static readonly PowerModifierFormula AbilityModifierDamage = new AbilityModifierDamageFormula(GeneralKeyword);
        public static readonly PowerModifierFormula Multiple3x3 = new BurstFormula();
        public static readonly ImmutableList<PowerModifierFormula> modifiers = new PowerModifierFormula[]
        {
            AbilityModifierDamage,
            NonArmorDefense,
            Multiple3x3,
            new ToHitBonusFormula(AccuratePowerTemplateName),
            new ConditionFormula("Slowed", ConditionsPowerTemplateName),
            new ConditionFormula("Dazed", ConditionsPowerTemplateName),
            new ConditionFormula("Immobilized", ConditionsPowerTemplateName),
            new ConditionFormula("Weakened", ConditionsPowerTemplateName),
            new ConditionFormula("Grants Combat Advantage", ConditionsPowerTemplateName),
            new ImmediateConditionFormula("Prone", new PowerCost(1), ConditionsPowerTemplateName),
            new DefensePenaltyFormula(ConditionsPowerTemplateName),
            new ShiftFormula(SkirmishPowerTemplateName),
            new TempPowerModifierFormula(SkirmishPowerTemplateName, "Movement after Attack does not provoke opportunity attacks", new PowerCost(0.5)),
            new TempPowerModifierFormula(BonusPowerTemplateName, "To-Hit Bonus +2 (or Abil) to next attack (or to specific target)", new PowerCost(0.5)),
            new TempPowerModifierFormula(BonusPowerTemplateName, "+2 to AC to Ally", new PowerCost(0.5)),
            new TempPowerModifierFormula(BonusPowerTemplateName, "+Ability Bonus Temporary Hit points", new PowerCost(1)),
            new TempPowerModifierFormula(BonusPowerTemplateName, "Extra Saving Throw", new PowerCost(1)),
            new TempPowerModifierFormula(BonusPowerTemplateName, "Healing Surge", new PowerCost(1)), // TODO - Encounter only
            new TempPowerModifierFormula(BonusPowerTemplateName, "Regeneration 5", new PowerCost(1)), // TODO - Daily only
            // Blinded
            // Slowed/Unconscious
            // Ongoing
            // Reroll attack
            // Disarm and catch
            // Free basic attacks
            // Secondary burst (such as acid splash)
        }.ToImmutableList();

        public static IEnumerable<string> PowerModifierNames => modifiers.Select(v => v.Name);

        private record NonArmorDefenseFormula(ImmutableList<string> Keywords) : PowerModifierFormula(Keywords, "Non-Armor Defense")
        {
            public NonArmorDefenseFormula(params string[] keywords) : this(keywords.ToImmutableList()) { }

            public override IEnumerable<ApplicablePowerModifierFormula> GetApplicable(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo)
            {
                if (HasModifier(attack)) yield break;
                var cost = powerInfo.ToolProfile.Type == ToolType.Implement ? new PowerCost(0) : new PowerCost(0.5);
                yield return new(cost, BuildModifier(powerInfo.ToolProfile.PrimaryNonArmorDefense), Chances: 10);
                yield return new(cost, BuildModifier(DefenseType.Fortitude), Chances: 1);
                yield return new(cost, BuildModifier(DefenseType.Reflex), Chances: 1);
                yield return new(cost, BuildModifier(DefenseType.Will), Chances: 1);

                PowerModifier BuildModifier(DefenseType defense) =>
                    new PowerModifier(Name, Build(("Defense", defense.ToString("g"))));
            }

            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile, PowerModifier modifier)
            {
                return ModifyAttack(effect, attack => attack with { Defense = Enum.Parse<DefenseType>(modifier.Options["Defense"]) });
            }
        }

        private record AbilityModifierDamageFormula(ImmutableList<string> Keywords) : PowerModifierFormula(Keywords, "Ability Modifier Damage")
        {
            public AbilityModifierDamageFormula(params string[] keywords) : this(keywords.ToImmutableList()) { }

            public override IEnumerable<ApplicablePowerModifierFormula> GetApplicable(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo)
            {
                if (HasModifier(attack)) yield break;
                // TODO - allow primary and secondary damage
                yield return new(new PowerCost(0.5), BuildModifier(powerInfo.ToolProfile.Abilities[0]));

                PowerModifier BuildModifier(Ability ability) =>
                    new PowerModifier(Name, Build(("Ability", ability.ToString("g"))));
            }

            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile, PowerModifier modifier)
            {
                return ModifyDamage(effect, damage =>
                {
                    var ability = Ability.Strength; // TODO - configure ability

                    var initial = damage[0];
                    var dice = GameDiceExpression.Parse(initial.Amount) + ability;

                    return damage.SetItem(0, initial with
                    {
                        Amount = dice.ToString(),
                    });
                });
            }
        }

        private record ToHitBonusFormula(ImmutableList<string> Keywords) : PowerModifierFormula(Keywords, "To-Hit Bonus to Current Attack")
        {
            public ToHitBonusFormula(params string[] keywords) : this(keywords.ToImmutableList()) { }

            public override IEnumerable<ApplicablePowerModifierFormula> GetApplicable(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo)
            {
                if (HasModifier(attack)) yield break;
                foreach (var entry in powerInfo.ToolProfile.Abilities.Where(a => a != attack.Ability))
                    yield return new(new PowerCost(0.5), BuildModifier((GameDiceExpression)entry), Chances: 1);
                yield return new(new PowerCost(0.5), BuildModifier(2), Chances: 5);

                PowerModifier BuildModifier(GameDiceExpression dice) =>
                    new PowerModifier(Name, Build(("Amount", dice.ToString())));
            }

            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile, PowerModifier modifier)
            {
                return ModifyAttack(effect, attack => attack with
                {
                    Bonus = (GameDiceExpression.Parse(attack.Bonus) + GameDiceExpression.Parse(modifier.Options["Amount"])).ToString()
                });
            }
        }

        private record ShiftFormula(ImmutableList<string> Keywords) : PowerModifierFormula(Keywords, "Shift")
        {
            public ShiftFormula(params string[] keywords) : this(keywords.ToImmutableList()) { }

            public override IEnumerable<ApplicablePowerModifierFormula> GetApplicable(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo)
            {
                if (HasModifier(attack)) yield break;
                // TODO - allow sliding allies, fixed numbers, etc.
                yield return new(new PowerCost(0.5), BuildModifier((GameDiceExpression)powerInfo.ToolProfile.Abilities[0]));

                PowerModifier BuildModifier(GameDiceExpression amount) =>
                    new PowerModifier(Name, Build(
                        ("Amount", amount.ToString())
                    ));
            }

            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile, PowerModifier modifier)
            {
                return effect with { Slide = new SerializedSlide(Amount: modifier.Options["Amount"]) };
            }
        }

        private record ConditionFormula(ImmutableList<string> Keywords, string Name, ImmutableDictionary<Duration, PowerCost> PowerCost) : PowerModifierFormula(Keywords, Name)
        {
            public ConditionFormula(string conditionName, params string[] keywords)
                : this(conditionName, Build((Duration.SaveEnds, new PowerCost(Fixed: 1)), (Duration.EndOfUserNextTurn, new PowerCost(Fixed: 0.5))), keywords)
            {
            }

            public ConditionFormula(string conditionName, ImmutableDictionary<Duration, PowerCost> powerCost, params string[] keywords)
                : this(keywords.ToImmutableList(), Name: conditionName, PowerCost: powerCost)
            {
            }

            public override IEnumerable<ApplicablePowerModifierFormula> GetApplicable(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo)
            {
                if (HasModifier(attack)) yield break;
                var prevThreshold = 0;
                foreach (var powerCost in PowerCost.EscalatingOdds())
                {
                    yield return new(powerCost.result.Value, BuildModifier(powerCost.result.Key), Chances: powerCost.threshold - prevThreshold);
                    prevThreshold = powerCost.threshold;
                }

                PowerModifier BuildModifier(Duration duration) =>
                    new PowerModifier(Name, Build(
                        ("Duration", duration.ToString("g"))
                    ));
            }

            private IEnumerable<(PowerCost cost, Duration duration)> GetAvailableOptions(AttackProfileBuilder attack)
            {
                return from kvp in PowerCost
                       orderby kvp.Key descending
                       where attack.Cost.CanApply(kvp.Value)
                       select (cost: kvp.Value, duration: kvp.Key);
            }

            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile, PowerModifier modifier)
            {
                // TODO
                return effect;
            }
        }

        private record ImmediateConditionFormula(ImmutableList<string> Keywords, string Name, PowerCost Cost) : PowerModifierFormula(Keywords, Name)
        {
            public ImmediateConditionFormula(string conditionName, PowerCost cost, params string[] keywords)
                : this(keywords.ToImmutableList(), Name: conditionName, Cost: cost)
            {
            }

            public override IEnumerable<ApplicablePowerModifierFormula> GetApplicable(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo)
            {
                if (HasModifier(attack)) yield break;

                yield return new(Cost, BuildModifier());

                PowerModifier BuildModifier() =>
                    new PowerModifier(Name);
            }

            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile, PowerModifier modifier)
            {
                // TODO
                return effect;
            }
        }

        private record BurstFormula() : PowerModifierFormula(Build(GeneralKeyword), "Multiple")
        {

            public override IEnumerable<ApplicablePowerModifierFormula> GetApplicable(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo)
            {
                if (HasModifier(attack)) yield break;

                // TODO: other
                var sizes = new[]
                {
                    (Cost: new PowerCost(Multiplier: 2.0 / 3), Size: "3x3"),
                };

                foreach (var size in sizes)
                {
                    if (attack.Target != TargetType.Range || powerInfo.ToolProfile.Type != ToolType.Weapon)
                        yield return new(size.Cost, BuildModifier(type: "Burst", size: size.Size));
                    if (attack.Target != TargetType.Melee || powerInfo.ToolProfile.Type != ToolType.Weapon)
                    {
                        yield return new(size.Cost, BuildModifier(type: "Blast", size: size.Size));
                        yield return new(size.Cost, BuildModifier(type: "Area", size: size.Size));
                    }
                }

                PowerModifier BuildModifier(string type, string size) =>
                    new PowerModifier(Name, Build(("Size", size), ("Type", type)));
            }

            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile, PowerModifier modifier)
            {
                return ModifyTarget(effect, target =>
                {
                    return target with { Burst = 3 }; // TODO - more sizes
                });
            }
        }

        private record DefensePenaltyFormula(ImmutableList<string> Keywords) : PowerModifierFormula(Keywords, "-2 to Defense")
        {
            public DefensePenaltyFormula(params string[] keywords) : this(keywords.ToImmutableList()) { }

            public override IEnumerable<ApplicablePowerModifierFormula> GetApplicable(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo)
            {
                if (HasModifier(attack)) yield break;
                yield return new(new PowerCost(0.5), BuildModifier(DefenseType.ArmorClass, Duration.EndOfUserNextTurn));
                yield return new(new PowerCost(0.5), BuildModifier(DefenseType.Fortitude, Duration.EndOfUserNextTurn));
                yield return new(new PowerCost(0.5), BuildModifier(DefenseType.Reflex, Duration.EndOfUserNextTurn));
                yield return new(new PowerCost(0.5), BuildModifier(DefenseType.Will, Duration.EndOfUserNextTurn));
                yield return new(new PowerCost(1), BuildModifier(DefenseType.ArmorClass, Duration.SaveEnds));
                yield return new(new PowerCost(1), BuildModifier(DefenseType.Fortitude, Duration.SaveEnds));
                yield return new(new PowerCost(1), BuildModifier(DefenseType.Reflex, Duration.SaveEnds));
                yield return new(new PowerCost(1), BuildModifier(DefenseType.Will, Duration.SaveEnds));

                yield return new(new PowerCost(1), BuildModifier(null, Duration.EndOfUserNextTurn));
                yield return new(new PowerCost(0, Multiplier: 0.5), BuildModifier(null, Duration.SaveEnds));

                PowerModifier BuildModifier(DefenseType? defense, Duration duration) =>
                    new PowerModifier(Name, Build(
                        ("Defense", defense?.ToString("g") ?? "All"),
                        ("Duration", duration.ToString("g"))
                    ));
            }

            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile, PowerModifier modifier)
            {
                // TODO
                return effect;
            }
        }

    }
}
