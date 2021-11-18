using GameEngine.Rules;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace GameEngine.Generator
{
    public static class ProseHelpers
    { 
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
            if (!text.EndsWith("."))
                return (text + ".");
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
