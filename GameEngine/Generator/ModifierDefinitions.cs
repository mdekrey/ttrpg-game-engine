using System;
using System.Collections.Generic;
using System.Text;
using static GameEngine.Generator.PowerModifierFormulaPredicates;
using static GameEngine.Generator.PowerDefinitions;
using System.Collections.Immutable;
using GameEngine.Rules;
using System.Linq;

namespace GameEngine.Generator
{
    public static class ModifierDefinitions
    {
        public const string GeneralKeyword = "General";

        public static readonly PowerModifierFormula NonArmorDefense = new NonArmorDefenseFormula(AccuratePowerTemplateName);
        public static readonly PowerModifierFormula AbilityModifierDamage = new AbilityModifierDamageFormula(GeneralKeyword);
        public static readonly PowerModifierFormula Multiple3x3 = new TempPowerModifierFormula(GeneralKeyword, "Multiple3x3", new CostMultiplier(2.0 / 3), And(MinimumPower(1.5), MaxOccurrence(1))); // TODO - other sizes
        public static readonly ImmutableList<PowerModifierFormula> modifiers = new PowerModifierFormula[]
        {
            AbilityModifierDamage,
            NonArmorDefense,
            new TempPowerModifierFormula(AccuratePowerTemplateName, "To-Hit Bonus +2", new FlatCost(0.5), And(MinimumPower(1.5), MaxOccurrence(1))),
            new ConditionFormula("Slowed", ConditionsPowerTemplateName),
            new ConditionFormula("Dazed", ConditionsPowerTemplateName),
            new ConditionFormula("Immobilized", ConditionsPowerTemplateName),
            new ConditionFormula("Weakened", ConditionsPowerTemplateName),
            new ConditionFormula("Grants Combat Advantage", ConditionsPowerTemplateName),
            new ImmediateConditionFormula("Prone", new FlatCost(1), ConditionsPowerTemplateName),
            new TempPowerModifierFormula(ConditionsPowerTemplateName, "-2 to One Defense", new FlatCost(0.5), And(MinimumPower(1.5), MaxOccurrence(1))), // TODO - combine these with options
            new TempPowerModifierFormula(ConditionsPowerTemplateName, "-2 (or Abil) to all Defenses", new FlatCost(1), And(MinimumPower(1.5), MaxOccurrence(1))), // TODO - combine these with options
            new ShiftFormula(SkirmishPowerTemplateName),
            new TempPowerModifierFormula(SkirmishPowerTemplateName, "Movement after Attack does not provoke opportunity attacks", new FlatCost(0.5), And(MinimumPower(1.5), MaxOccurrence(1))),
            new TempPowerModifierFormula(BonusPowerTemplateName, "To-Hit Bonus +2 (or Abil) to next attack (or to specific target)", new FlatCost(0.5), And(MinimumPower(1.5), MaxOccurrence(1))),
            new TempPowerModifierFormula(BonusPowerTemplateName, "+2 to AC to Ally", new FlatCost(0.5), And(MinimumPower(1.5), MaxOccurrence(1))),
            new TempPowerModifierFormula(BonusPowerTemplateName, "+Ability Bonus Temporary Hit points", new FlatCost(1), And(MinimumPower(1.5), MaxOccurrence(1))),
            new TempPowerModifierFormula(BonusPowerTemplateName, "Extra Saving Throw", new FlatCost(1), And(MinimumPower(1.5), MaxOccurrence(1))),
            new TempPowerModifierFormula(BonusPowerTemplateName, "Healing Surge", new FlatCost(1), And(MinimumPower(2), MaxOccurrence(1), MaximumFrequency(PowerFrequency.Encounter))),
            new TempPowerModifierFormula(BonusPowerTemplateName, "Regeneration 5", new FlatCost(1), And(MinimumPower(2), MaxOccurrence(2), MaximumFrequency(PowerFrequency.Daily))),
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

            public override bool CanApply(AttackProfile attack, PowerHighLevelInfo powerInfo) => And(MinimumPower(1.5), MaxOccurrence(1))(this, attack, powerInfo);

            public override AttackProfile Apply(AttackProfile attack, PowerHighLevelInfo powerInfo, RandomGenerator randomGenerator)
            {
                var defense = randomGenerator.RandomSelection((10, powerInfo.ToolProfile.PrimaryNonArmorDefense), (1, DefenseType.Fortitude), (1, DefenseType.Reflex), (1, DefenseType.Will));
                return Apply(
                    attack, 
                    powerInfo.ToolProfile.Type == ToolType.Implement ? new FlatCost(0) : new FlatCost(0.5),
                    new PowerModifier(
                        Name, 
                        ImmutableDictionary<string, string>.Empty
                            .Add("Defense", defense.ToString("g"))
                    )
                );
            }

            public override SerializedEffect Apply(SerializedEffect attack, PowerProfile powerProfile, AttackProfile attackProfile)
            {
                throw new NotImplementedException();
            }
        }

        private record AbilityModifierDamageFormula(ImmutableList<string> Keywords) : PowerModifierFormula(Keywords, "Ability Modifier Damage")
        {
            public AbilityModifierDamageFormula(params string[] keywords) : this(keywords.ToImmutableList()) { }

            public override bool CanApply(AttackProfile attack, PowerHighLevelInfo powerInfo) => And(MinimumPower(1.5), MaxOccurrence(2))(this, attack, powerInfo);

            public override AttackProfile Apply(AttackProfile attack, PowerHighLevelInfo powerInfo, RandomGenerator randomGenerator) =>
                Apply(attack, new FlatCost(0.5), new PowerModifier(Name, ImmutableDictionary<string, string>.Empty));

            public override SerializedEffect Apply(SerializedEffect attack, PowerProfile powerProfile, AttackProfile attackProfile)
            {
                return ModifyDamage(attack, damage =>
                {
                    var ability = Ability.Strength; // TODO - configure ability

                    var initial = damage[0];
                    var dice = GameDiceExpression.Parse(initial.Amount);

                    return damage.SetItem(0, initial with
                    {
                        Amount = (dice with { Abilities = dice.Abilities.With(ability, dice.Abilities[ability] + 1) }).ToString(),
                    });
                });
            }
        }

        private record ShiftFormula(ImmutableList<string> Keywords) : PowerModifierFormula(Keywords, "Shift")
        {
            public ShiftFormula(params string[] keywords) : this(keywords.ToImmutableList()) { }

            public override bool CanApply(AttackProfile attack, PowerHighLevelInfo powerInfo) => And(MinimumPower(1.5), MaxOccurrence(1))(this, attack, powerInfo);

            public override AttackProfile Apply(AttackProfile attack, PowerHighLevelInfo powerInfo, RandomGenerator randomGenerator) =>
                Apply(attack, new FlatCost(0.5), new PowerModifier(Name, ImmutableDictionary<string, string>.Empty));

            public override SerializedEffect Apply(SerializedEffect attack, PowerProfile powerProfile, AttackProfile attackProfile)
            {
                var ability = Ability.Dexterity; // TODO - configure ability
                return attack with { Slide = new SerializedSlide(Amount: (GameDiceExpression.Empty with { Abilities = CharacterAbilities.Empty.With(ability, 1) }).ToString()) };
            }
        }

        private record ConditionFormula(ImmutableList<string> Keywords, string Name, ImmutableDictionary<Duration, IPowerCost> PowerCost) : PowerModifierFormula(Keywords, Name)
        {
            public ConditionFormula(string conditionName, params string[] keywords)
                : this(conditionName, new[] { (Duration.SaveEnds, (IPowerCost)new FlatCost(1)), (Duration.EndOfUserNextTurn, new FlatCost(0.5)) }, keywords)
            {
            }

            public ConditionFormula(string conditionName, IReadOnlyList<(Duration duration, IPowerCost cost)> powerCost, params string[] keywords) 
                : this(keywords.ToImmutableList(), Name: conditionName, PowerCost: powerCost.ToImmutableDictionary(p => p.duration, p => p.cost)) 
            {
            }

            public override bool CanApply(AttackProfile attack, PowerHighLevelInfo powerInfo) => GetAvailableOptions(attack).Any() && attack.Modifiers.Count(m => m.Modifier == Name) < 1;

            public override AttackProfile Apply(AttackProfile attack, PowerHighLevelInfo powerInfo, RandomGenerator randomGenerator)
            {
                IEnumerable<(IPowerCost cost, Duration duration)> options = GetAvailableOptions(attack);

                var selectedOption = randomGenerator.RandomEscalatingSelection(options);

                return Apply(
                    attack,
                    selectedOption.cost,
                    new PowerModifier(
                        Name,
                        ImmutableDictionary<string, string>.Empty
                            .Add("Duration", selectedOption.duration.ToString("g"))
                    )
                );
            }

            private IEnumerable<(IPowerCost cost, Duration duration)> GetAvailableOptions(AttackProfile attack)
            {
                return from kvp in PowerCost
                       orderby kvp.Key descending
                       where kvp.Value.Apply(attack.WeaponDice) >= 1
                       select (cost: kvp.Value, duration: kvp.Key);
            }

            public override SerializedEffect Apply(SerializedEffect attack, PowerProfile powerProfile, AttackProfile attackProfile)
            {
                // TODO
                return attack;
            }
        }

        private record ImmediateConditionFormula(ImmutableList<string> Keywords, string Name, IPowerCost Cost) : PowerModifierFormula(Keywords, Name)
        {
            public ImmediateConditionFormula(string conditionName, IPowerCost cost, params string[] keywords)
                : this(keywords.ToImmutableList(), Name: conditionName, Cost: cost)
            {
            }

            public override bool CanApply(AttackProfile attack, PowerHighLevelInfo powerInfo) => Cost.Apply(attack.WeaponDice) >= 1 && attack.Modifiers.Count(m => m.Modifier == Name) < 1;

            public override AttackProfile Apply(AttackProfile attack, PowerHighLevelInfo powerInfo, RandomGenerator randomGenerator)
            {
                return Apply(
                    attack,
                    Cost,
                    new PowerModifier(
                        Name,
                        ImmutableDictionary<string, string>.Empty
                    )
                );
            }

            public override SerializedEffect Apply(SerializedEffect attack, PowerProfile powerProfile, AttackProfile attackProfile)
            {
                // TODO
                return attack;
            }
        }
    }
}
