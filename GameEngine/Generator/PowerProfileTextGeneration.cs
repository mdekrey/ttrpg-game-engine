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

            var attacks = profile.Attacks.Select((attack, index) => attack.ToAttackTextBlock(powerHighLevelInfo, index + 1)).ToArray();
            result = result.AddAttack(attacks[0], 1);
            // TODO - add power modifiers
            result = attacks.Select((attack, index) => (attack, index)).Skip(1).Aggregate(result, (powerBlock, attackBlock) => powerBlock.AddAttack(attackBlock.attack, attackBlock.index + 1));

            return result;
        }

        public static AttackTextBlock ToAttackTextBlock(this AttackProfile profile, PowerHighLevelInfo powerHighLevelInfo, int index)
        {
            var dice = PowerProfileExtensions.ToDamageEffect(powerHighLevelInfo.ToolProfile.Type, profile.WeaponDice);
            var result = new AttackTextBlock(
                Type: profile.Target.ToText(),
                Target: "One creature", // TODO - different target line for non-primary attacks
                Attack: $"{profile.Ability:g} vs. AC",
                Hit: $"{dice} {OxfordComma(profile.DamageTypes.Where(d => d != DamageType.Normal).Select(d => d.ToText().ToLower()).ToArray())} damage",
                Miss: null
            );
            // TODO - attack modifiers
            return result with
            {
                Hit = result.Hit.FinishSentence(),
                Miss = result.Miss == null ? null : result.Miss.FinishSentence(),
            };
        }

        private static PowerTextBlock AddAttack(this PowerTextBlock power, AttackTextBlock attack, int index)
        {
            if (power.AttackType == null)
            {
                if (index != 1)
                    throw new ArgumentException();
                return power with
                {
                    AttackType = attack.Type,
                    Target = attack.Target,
                    Attack = attack.Attack,
                    RulesText = power.RulesText.Items
                        .Add(new("Hit", attack.Hit))
                        .AddIf(attack.Miss != null, new("Miss", attack.Miss!)),
                };
            }
            else
            {
                return power with
                {
                    RulesText = power.RulesText.Items
                        .Add(new($"{Ordinal(index).Capitalize()} Target", attack.Target))
                        .Add(new($"{Ordinal(index).Capitalize()} Hit", attack.Hit))
                        .AddIf(attack.Miss != null, new($"{Ordinal(index).Capitalize()} Miss", attack.Miss!))
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

        private static string FinishSentence(this string text)
        {
            if (!text.EndsWith("."))
                return text + ".";
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

        private static ImmutableList<T> AddIf<T>(this ImmutableList<T> list, bool conditional, T item) =>
            conditional ? list.Add(item) : list;

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
