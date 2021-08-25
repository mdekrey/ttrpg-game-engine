using GameEngine.Rules;
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

        internal static AttackType From(ToolProfile toolProfile)
        {
            return toolProfile switch
            {
                { Type: ToolType.Weapon, Range: ToolRange.Melee } => new MeleeWeaponAttackType(),
                { Type: ToolType.Implement, Range: ToolRange.Melee } => new MeleeTouchAttackType(),
                { Type: ToolType.Weapon, Range: ToolRange.Range } => new RangedWeaponAttackType(),
                { Type: ToolType.Implement, Range: ToolRange.Range } => new RangedAttackType(10),
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
    public record RangedSightAttackType(int Range) : AttackType()
    {
        public override string TypeText() => $"Ranged sight";
        public override string AdditionalTargetText() => "One creature other than the primary target"; // TODO
    }
    // TODO - close burst
    // TODO - close blast
    // TODO - area burst
    // TODO - area wall
    // TODO - personal

    public record AttackInfo(
        AttackType AttackType,
        AttackInfo.Target TargetType,
        GameDiceExpression AttackExpression,
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

        internal string ToAttackText() => $"{this.AttackExpression} vs. {this.Defense.ToText()}";
    }


    public static class PowerProfileTextGeneration
    {
        public static PowerTextBlock ToPowerTextBlock(this PowerProfile profile, PowerHighLevelInfo powerHighLevelInfo)
        {
            var result = new PowerTextBlock(
                Name: "Unknown",
                TypeInfo: $"{powerHighLevelInfo.ToolProfile.Type:g} Attack {powerHighLevelInfo.Level}",
                FlavorText: null,
                PowerUsage: powerHighLevelInfo.Usage.ToText(),
                PowerKeywords: ImmutableList<string>.Empty.Add(powerHighLevelInfo.ToolProfile.Type.ToKeyword()),
                ActionType: "Standard Action",
                AttackType: null,
                AttackTypeDetails: null,
                Prerequisite: null,
                Requirement: null,
                Target: null,
                Attack: null,
                RulesText: ImmutableList<RulesText>.Empty
            );

            var attacks = profile.Attacks.Select((attack, index) => attack.ToAttackInfo(powerHighLevelInfo, index + 1)).ToArray();
            result = result.AddAttack(attacks[0], 1);
            result = (from mod in profile.Modifiers
                     let mutator = mod.GetTextMutator()
                     orderby mutator.Priority
                     select mutator.Apply).Aggregate(result, (current, apply) => apply(current, powerHighLevelInfo));
            result = attacks.Select((attack, index) => (attack, index)).Skip(1).Aggregate(result, (powerBlock, attackBlock) => powerBlock.AddAttack(attackBlock.attack, attackBlock.index + 1));

            return result with
            {
                RulesText = result.RulesText.Items.Where(rule => rule.Text is { Length: > 0 }).ToImmutableList(),
            };
        }

        public static AttackInfo ToAttackInfo(this AttackProfile profile, PowerHighLevelInfo powerHighLevelInfo, int index)
        {
            var dice = PowerProfileExtensions.ToDamageEffect(powerHighLevelInfo.ToolProfile.Type, profile.WeaponDice);
            var result = new AttackInfo(
                AttackType: AttackType.From(powerHighLevelInfo.ToolProfile),
                TargetType: AttackInfo.Target.OneCreature,
                AttackExpression: (GameDiceExpression)profile.Ability,
                Defense: DefenseType.ArmorClass,
                DamageExpression: dice,
                DamageTypes: profile.DamageTypes,
                HitParts: ImmutableList<string>.Empty,
                HitSentences: ImmutableList<string>.Empty,
                MissParts: ImmutableList<string>.Empty
            );
            result = (from mod in profile.Modifiers
                     let mutator = mod.GetAttackInfoMutator()
                     orderby mutator.Priority
                     select mutator.Apply).Aggregate(result, (current, apply) => apply(current, powerHighLevelInfo, index));
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
