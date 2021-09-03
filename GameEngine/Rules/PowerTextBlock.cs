using GameEngine.Generator;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GameEngine.Rules
{
    public record PowerTextBlock(
        string Name, string TypeInfo, 
        string? FlavorText,
        string PowerUsage, EquatableImmutableList<string> Keywords, 
        string? ActionType, string? AttackType, string? AttackTypeDetails, 
        string? Prerequisite, string? Requirement, string? Trigger,
        string? Target,
        string? Attack,
        EquatableImmutableList<RulesText> RulesText
    );

    public record RulesText(
        string Label,
        string Text
    );

    public static class RulesTextExtensions
    {
        public static ImmutableList<RulesText> AddEffectSentences(this EquatableImmutableList<RulesText> rulesText, IEnumerable<string> sentences)
        {
            var index = rulesText.Items.FindIndex(r => r.Label == "Effect");
            if (index == -1)
                return rulesText.Items.Add(new RulesText("Effect", string.Join(' ', sentences)));
            return rulesText.Items.SetItem(index, rulesText.Items[index] with
            {
                Text = string.Join(' ', new[] { rulesText.Items[index].Text }.Concat(sentences))
            });
        }
    }
}
