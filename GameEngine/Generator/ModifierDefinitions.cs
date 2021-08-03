﻿using System;
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
            new TempPowerModifierFormula(AccuratePowerTemplateName, "To-Hit Bonus +2", new PowerCost(0.5)),
            new ConditionFormula("Slowed", ConditionsPowerTemplateName),
            new ConditionFormula("Dazed", ConditionsPowerTemplateName),
            new ConditionFormula("Immobilized", ConditionsPowerTemplateName),
            new ConditionFormula("Weakened", ConditionsPowerTemplateName),
            new ConditionFormula("Grants Combat Advantage", ConditionsPowerTemplateName),
            new ImmediateConditionFormula("Prone", new PowerCost(1), ConditionsPowerTemplateName),
            new TempPowerModifierFormula(ConditionsPowerTemplateName, "-2 to One Defense", new PowerCost(0.5)), // TODO - combine these with options
            new TempPowerModifierFormula(ConditionsPowerTemplateName, "-2 (or Abil) to all Defenses", new PowerCost(1)), // TODO - combine these with options
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

            public override AttackProfileBuilder Apply(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo, RandomGenerator randomGenerator)
            {
                var defense = randomGenerator.RandomSelection((10, powerInfo.ToolProfile.PrimaryNonArmorDefense), (1, DefenseType.Fortitude), (1, DefenseType.Reflex), (1, DefenseType.Will));
                return Apply(
                    attack,
                    powerInfo.ToolProfile.Type == ToolType.Implement ? new PowerCost(0) : new PowerCost(0.5),
                    new PowerModifier(
                        Name,
                        ImmutableDictionary<string, string>.Empty
                            .Add("Defense", defense.ToString("g"))
                    )
                );
            }

            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile, PowerModifier modifier)
            {
                return ModifyAttack(effect, attack => attack with { Defense = Enum.Parse<DefenseType>(modifier.Options["Defense"]) });
            }
        }

        private record AbilityModifierDamageFormula(ImmutableList<string> Keywords) : PowerModifierFormula(Keywords, "Ability Modifier Damage")
        {
            public AbilityModifierDamageFormula(params string[] keywords) : this(keywords.ToImmutableList()) { }

            // TODO - allow primary and secondary damage
            public override AttackProfileBuilder Apply(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo, RandomGenerator randomGenerator) =>
                Apply(attack, new PowerCost(0.5), new PowerModifier(Name, ImmutableDictionary<string, string>.Empty));

            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile, PowerModifier modifier)
            {
                return ModifyDamage(effect, damage =>
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

            public override AttackProfileBuilder Apply(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo, RandomGenerator randomGenerator) =>
                Apply(attack, new PowerCost(0.5), new PowerModifier(Name, ImmutableDictionary<string, string>.Empty));

            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile, PowerModifier modifier)
            {
                var ability = Ability.Dexterity; // TODO - configure ability
                return effect with { Slide = new SerializedSlide(Amount: (GameDiceExpression.Empty with { Abilities = CharacterAbilities.Empty.With(ability, 1) }).ToString()) };
            }
        }

        private record ConditionFormula(ImmutableList<string> Keywords, string Name, ImmutableDictionary<Duration, PowerCost> PowerCost) : PowerModifierFormula(Keywords, Name)
        {
            public ConditionFormula(string conditionName, params string[] keywords)
                : this(conditionName, new[] { (Duration.SaveEnds, new PowerCost(Fixed: 1)), (Duration.EndOfUserNextTurn, new PowerCost(Fixed: 0.5)) }, keywords)
            {
            }

            public ConditionFormula(string conditionName, IReadOnlyList<(Duration duration, PowerCost cost)> powerCost, params string[] keywords)
                : this(keywords.ToImmutableList(), Name: conditionName, PowerCost: powerCost.ToImmutableDictionary(p => p.duration, p => p.cost))
            {
            }

            public override bool CanApply(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo) => GetAvailableOptions(attack).Any() && attack.Modifiers.Count(m => m.Modifier == Name) < 1;

            public override AttackProfileBuilder Apply(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo, RandomGenerator randomGenerator)
            {
                IEnumerable<(PowerCost cost, Duration duration)> options = GetAvailableOptions(attack);

                var selectedOption = randomGenerator.RandomEscalatingSelection(options);

                return Apply(
                    attack,
                    selectedOption.cost,
                    new PowerModifier(
                        Name,
                        Build(("Duration", selectedOption.duration.ToString("g")))
                    )
                );
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

            public override bool CanApply(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo) => attack.Cost.CanApply(Cost) && base.CanApply(attack, powerInfo);

            public override AttackProfileBuilder Apply(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo, RandomGenerator randomGenerator)
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

            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile, PowerModifier modifier)
            {
                // TODO
                return effect;
            }
        }

        private record BurstFormula() : PowerModifierFormula(Build(GeneralKeyword), "Multiple")
        {
            public override AttackProfileBuilder Apply(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo, RandomGenerator randomGenerator)
            {
                // TODO: other sizes
                return Apply(
                    attack,
                    new PowerCost(Multiplier: 2.0 / 3),
                    new PowerModifier(Name, Build(("Size", "3x3")))
                );
            }

            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile, PowerModifier modifier)
            {
                return ModifyTarget(effect, target =>
                {
                    return target with { Burst = 3 }; // TODO - more sizes
                });
            }
        }
    }
}