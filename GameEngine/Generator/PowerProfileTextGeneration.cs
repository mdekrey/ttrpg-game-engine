﻿using GameEngine.Rules;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace GameEngine.Generator
{
    public abstract record AttackType()
    {
        public abstract string TypeText();
        public abstract string AdditionalTargetText();

        internal static AttackType From(ToolType weapon, ToolRange range)
        {
            return (weapon, range) switch
            {
                (ToolType.Weapon, ToolRange.Melee) => new MeleeWeaponAttackType(),
                (ToolType.Implement, ToolRange.Melee) => new MeleeTouchAttackType(),
                (ToolType.Weapon, ToolRange.Range) => new RangedWeaponAttackType(),
                (ToolType.Implement, ToolRange.Range) => new RangedAttackType(10),
                _ => throw new NotSupportedException(),
            };
        }
    }
    public record MeleeWeaponAttackType() : AttackType()
    {
        public override string TypeText() => "Melee weapon";
        public override string AdditionalTargetText() => "One creature other than the primary target"; // TODO
    }
    public record MeleeTouchAttackType() : AttackType()
    {
        public override string TypeText() => "Melee touch";
        public override string AdditionalTargetText() => "One creature other than the primary target"; // TODO
    }
    public record RangedWeaponAttackType() : AttackType()
    {
        public override string TypeText() => $"Ranged weapon";
        public override string AdditionalTargetText() => "One creature other than the primary target"; // TODO
    }
    public record RangedAttackType(int Range) : AttackType()
    {
        public override string TypeText() => $"Ranged {Range}";
        public override string AdditionalTargetText() => "One creature other than the primary target"; // TODO
    }
    public record RangedSightAttackType() : AttackType()
    {
        public override string TypeText() => $"Ranged sight";
        public override string AdditionalTargetText() => "One creature other than the primary target"; // TODO
    }
    public record CloseBurst(int Range) : AttackType()
    {
        public override string TypeText() => $"Close burst {Range}";
        public override string AdditionalTargetText() => Range switch
        {
            1 => "All creatures adjacent to the primary target",
            _ => $"All creatures within {Range} of the primary target"
        }; // TODO - all enemies instead of "creatures"?
    }
    public record CloseBlast(int Range) : AttackType()
    {
        public override string TypeText() => $"Close blast {Range}";
        public override string AdditionalTargetText() => Range switch
        {
            1 => "All creatures adjacent to the primary target",
            _ => $"All creatures within {Range} of the primary target"
        }; // TODO - all enemies instead of "creatures"?
    }
    public record AreaBurst(int Size, int Range) : AttackType()
    {
        public override string TypeText() => $"Area burst {Size} within {Range}";
        public override string AdditionalTargetText() => Size switch
        {
            1 => "All creatures adjacent to the primary target",
            _ => $"All creatures within {Size} of the primary target"
        }; // TODO - all enemies instead of "creatures"?
    }
    // TODO - personal

    public record AttackInfo(
        AttackType AttackType,
        AttackInfo.Target TargetType,
        GameDiceExpression AttackExpression,
        string? AttackNotes,
        DefenseType Defense,
        GameDiceExpression DamageExpression,
        ImmutableList<DamageType> DamageTypes,
        ImmutableList<string> HitParts,
        ImmutableList<string> HitSentences,
        ImmutableList<string> MissParts
    )
    {
        public string Hit =>
            string.Join(" ",
                new[] {
                    PowerProfileTextGeneration.OxfordComma(Enumerable.Concat(new[] {
                        string.Join(" ", new string[]
                        {
                            DamageExpression.ToString(),
                            PowerProfileTextGeneration.OxfordComma(DamageTypes.Where(d => d != DamageType.Normal).Select(d => d.ToText().ToLower()).ToArray()),
                            "damage"
                        }.Where(s => s is { Length: > 0 }))
                    }, HitParts).ToArray()).FinishSentence().TransposeParenthesis()
                }.Concat(HitSentences)
            );

        public string Miss =>
            MissParts.Count == 0 ? "" : PowerProfileTextGeneration.OxfordComma(MissParts.ToArray()).FinishSentence().TransposeParenthesis();

        public enum Target
        {
            OneCreature,
            EachEnemy,
            YouOrOneAlly,
            EachAlly,
            OneOrTwoCreatures,
            OneTwoOrThreeCreatures,
        }

        internal string ToAttackText() => $"{this.AttackExpression} vs. {this.Defense.ToText()}"
            + AttackNotes ?? "";
    }


    public static class PowerProfileTextGeneration
    {
        public static PowerTextBlock ToPowerTextBlock(this PowerProfile profile)
        {
            var result = new PowerTextBlock(
                Name: "Unknown",
                TypeInfo: $"{profile.Tool.ToText()} Attack {profile.Level}",
                FlavorText: null,
                PowerUsage: profile.Usage.ToText(),
                PowerKeywords: ImmutableList<string>.Empty.Add(profile.Tool.ToKeyword()),
                ActionType: "Standard Action",
                AttackType: null,
                AttackTypeDetails: null,
                Prerequisite: null,
                Requirement: null,
                Target: null,
                Attack: null,
                RulesText: ImmutableList<RulesText>.Empty
            );

            var attacks = profile.Attacks.Select((attack, index) => attack.ToAttackInfo(profile, index + 1)).ToArray();
            result = result.AddAttack(attacks[0], 1);
            result = (from mod in profile.Modifiers
                     let mutator = mod.GetTextMutator()
                      where mutator != null
                      orderby mutator.Priority
                     select mutator.Apply).Aggregate(result, (current, apply) => apply(current, profile));
            result = attacks.Select((attack, index) => (attack, index)).Skip(1).Aggregate(result, (powerBlock, attackBlock) => powerBlock.AddAttack(attackBlock.attack, attackBlock.index + 1));

            return result with
            {
                RulesText = result.RulesText.Items.Where(rule => rule.Text is { Length: > 0 }).ToImmutableList(),
            };
        }

        public static AttackInfo ToAttackInfo(this AttackProfile profile, PowerProfile power, int index)
        {
            var dice = PowerProfileExtensions.ToDamageEffect(power.Tool, profile.WeaponDice);
            var result = new AttackInfo(
                AttackType: AttackType.From(power.Tool, power.ToolRange),
                TargetType: AttackInfo.Target.OneCreature,
                AttackExpression: (GameDiceExpression)profile.Ability,
                AttackNotes: null,
                Defense: DefenseType.ArmorClass,
                DamageExpression: dice,
                DamageTypes: profile.DamageTypes,
                HitParts: ImmutableList<string>.Empty,
                HitSentences: ImmutableList<string>.Empty,
                MissParts: ImmutableList<string>.Empty
            );
            result = (from mod in profile.Modifiers
                      let mutator = mod.GetAttackInfoMutator()
                      where mutator != null
                      orderby mutator.Priority
                      select mutator.Apply).Aggregate(result, (current, apply) => apply(current, power, index));
            return result;
        }

        private static PowerTextBlock AddAttack(this PowerTextBlock power, AttackInfo attack, int index)
        {
            if (power.AttackType == null)
            {
                if (index != 1)
                    throw new ArgumentException();
                return power with
                {
                    AttackType = attack.AttackType.TypeText(),
                    Target = attack.TargetType switch
                    {
                        // TODO - this should be dependent on the attack type AND target type - "each enemy in range" doesn't make sense for Area Burst attacks.
                        AttackInfo.Target.OneCreature => "One creature",
                        AttackInfo.Target.EachEnemy => "Each enemy in range",
                        AttackInfo.Target.YouOrOneAlly => "You or one ally",
                        AttackInfo.Target.EachAlly => "Each ally in range",
                        AttackInfo.Target.OneOrTwoCreatures => "One or two creatures",
                        AttackInfo.Target.OneTwoOrThreeCreatures => "One, two, or three creatures",
                        _ => throw new NotImplementedException(),
                    },
                    Attack = attack.ToAttackText(),
                    RulesText = power.RulesText.Items
                        .Add(new("Hit", attack.Hit))
                        .Add(new("Miss", attack.Miss)),
                };
            }
            else
            {
                // TODO: add "Make a secondary attack" message
                return power with
                {
                    RulesText = power.RulesText.Items
                        .Add(new($"{Ordinal(index).Capitalize()} Target", attack.TargetType switch
                        {
                            _ => "", // TODO
                        }))
                        .Add(new($"{Ordinal(index).Capitalize()} Attack", attack.ToAttackText()))
                        .Add(new($"{Ordinal(index).Capitalize()} Hit", attack.Hit))
                        .Add(new($"{Ordinal(index).Capitalize()} Miss", attack.Miss))
                };
            }
        }

        public static EquatableImmutableList<RulesText> AddSentence(this EquatableImmutableList<RulesText> rulesText, string label, string sentence)
        {
            if (rulesText.LastOrDefault(r => r.Label == label) is RulesText toReplace)
            {
                var result = rulesText.Items.Remove(toReplace);
                return result.Add(toReplace with { Text = toReplace.Text.FinishSentence() + " " + sentence });
            }
            else
            {
                return rulesText.Items.Add(new(label, sentence));
            }
        }

        public static string FinishSentence(this string text)
        {
            if (!text.EndsWith(".") && !text.EndsWith(".)"))
                return (text + ".").TransposeParenthesis();
            return text;
        }

        public static string TransposeParenthesis(this string text)
        {
            // English is weird, man.
            if (text.EndsWith(").") || text.EndsWith("),"))
                return text[0..^2] + text[^1] + ")";
            return text;
        }

        public static string OxfordComma(params string[] parts) =>
            parts switch
            {
                { Length: 0 } => "",
                { Length: 1 } => parts[0],
                { Length: 2 } => string.Join(" and ", parts),
                { Length: >= 3 } => string.Join(", ", parts.Take(parts.Length - 1)) + ", and " + parts[^1],
                _ => "",
            };

        public static string Ordinal(int index) =>
            index switch
            {
                0 => throw new NotSupportedException("You probably meant to use index + 1"),
                1 => "primary",
                2 => "secondary",
                3 => "tertiary",
                _ => throw new NotImplementedException("We don't use monikers that high up!"),
            };

        public static string Capitalize(this string s) =>
            s[0..1].ToUpper() + s[1..^0];

        public static string ToText(this Enum enumValue)
        {
            Type type = enumValue.GetType();
            FieldInfo info = type.GetField(enumValue.ToString());
            DescriptionAttribute[] da = (DescriptionAttribute[])(info.GetCustomAttributes(typeof(DescriptionAttribute), false));

            if (da.Length > 0)
                return da[0].Description;
            else
                return enumValue.ToString("g");
        }

    }
}
