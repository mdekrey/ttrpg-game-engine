﻿using GameEngine.Rules;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using static GameEngine.Generator.ProseHelpers;

namespace GameEngine.Generator.Text
{
    public static class PowerProfileTextGeneration
    {
        public static PowerTextBlock ToPowerTextBlock(this ClassPowerProfile profile)
        {
            var result = new PowerTextBlock(
                Name: "Unknown",
                TypeInfo: $"{profile.PowerProfile.Tool.ToText()} Attack {profile.Level}",
                FlavorText: null,
                PowerUsage: profile.PowerProfile.Usage.ToText(),
                Keywords: ImmutableList<string>.Empty.Add(profile.PowerProfile.Tool.ToKeyword()),
                ActionType: "Standard Action",
                AttackType: null,
                AttackTypeDetails: null,
                Prerequisite: null,
                Requirement: null,
                Trigger: null,
                Target: null,
                Attack: null,
                RulesText: ImmutableList<RulesText>.Empty
            );

            var attacks = profile.PowerProfile.Attacks.Select((attack, index) => attack.ToAttackInfo(profile.PowerProfile, index + 1)).ToArray();
            result = result.AddAttack(attacks[0], 1);
            result = (from mod in profile.PowerProfile.Modifiers
                      let mutator = mod.GetTextMutator(profile.PowerProfile)
                      where mutator != null
                      orderby mutator.Priority
                      select mutator.Apply).Aggregate(result, (current, apply) => apply(current, profile.PowerProfile));
            result = attacks.Select((attack, index) => (attack, index)).Skip(1).Aggregate(result, (powerBlock, attackBlock) => powerBlock.AddAttack(attackBlock.attack, attackBlock.index + 1));

            result = result with
            {
                RulesText = result.RulesText.AddEffectSentences(profile.PowerProfile.Effects.Select(effect => effect.ToTargetInfo(profile.PowerProfile, null).ToSentence()))
            };

            return result with
            {
                Keywords = result.Keywords.Items.OrderBy(k => k).ToImmutableList(),
                RulesText = result.RulesText.Items.Where(rule => rule.Text is { Length: > 0 }).ToImmutableList(),
            };
        }

        public static TargetInfo ToTargetInfo(this TargetEffect effect, PowerProfile power, int? attackIndex)
        {
            var result = new TargetInfo(
                Target: effect.Target.GetTargetText(power, attackIndex: attackIndex),
                Parts: ImmutableList<string>.Empty
            );

            result = (from mod in effect.Modifiers
                      let mutator = mod.GetTargetInfoMutator(effect, power)
                      where mutator != null
                      orderby mutator.Priority
                      select mutator.Apply).Aggregate(result, (current, apply) => apply(current));
            return result;
        }

        //public static string GetTargetText(this Target target, bool multiple)
        //{
        //    return (target, multiple) switch
        //    {
        //        (Target.Enemy, false) => "One enemy",
        //        (Target.Self, false) => "You",
        //        (Target.Self | Target.Enemy, false) => "You or one enemy", // This may be a good one for "If you take damage from this power, deal damage to all enemies instead." or something
        //        (Target.Ally, false) => "One of your allies",
        //        (Target.Ally | Target.Enemy, false) => "One creature other than yourself",
        //        (Target.Ally | Target.Self, false) => "You or one of your allies",
        //        (Target.Ally | Target.Self | Target.Enemy, false) => "One creature",

        //        (Target.Enemy, true) => "Each enemy",
        //        (Target.Self, true) => "You",
        //        (Target.Self | Target.Enemy, true) => "You and each enemy",
        //        (Target.Ally, true) => "Each of your allies",
        //        (Target.Ally | Target.Enemy, true) => "Each creature other than yourself",
        //        (Target.Ally | Target.Self, true) => "You and each of your allies",
        //        (Target.Ally | Target.Self | Target.Enemy, true) => "Each creature",

        //        _ => throw new NotImplementedException()
        //    };
        //}

        public static AttackInfo ToAttackInfo(this AttackProfile attack, PowerProfile power, int index)
        {
            var result = new AttackInfo(
                AttackType: AttackType.From(power.Tool, power.ToolRange),
                Target: "One creature",
                AttackExpression: (GameDiceExpression)attack.Ability,
                AttackNotes: null,
                Defense: DefenseType.ArmorClass,
                HitParts: ImmutableList<string>.Empty,
                HitSentences: ImmutableList<string>.Empty,
                MissSentences: ImmutableList<string>.Empty
            );
            var effects = attack.Effects.AsEnumerable();
            if (attack.Effects.Any() && attack.Effects[0].Target.GetTarget().HasFlag(Target.Enemy))
            {
                effects = effects.Skip(1);
                TargetInfo targetInfo = attack.Effects[0].ToTargetInfo(power, attackIndex: index);
                result = result with { HitParts = targetInfo.Parts };
            }
            result = result with { HitSentences = effects.Select(effect => effect.ToTargetInfo(power, attackIndex: index).ToSentence()).ToImmutableList() };
            // TODO - miss targets

            result = (from mod in attack.Modifiers
                      let mutator = mod.GetAttackInfoMutator(power)
                      where mutator != null
                      orderby mutator.Priority
                      select mutator.Apply).Aggregate(result, (current, apply) => apply(current, index));
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
                    AttackTypeDetails = attack.AttackType.TypeDetailsText(),
                    Target = attack.Target,
                    Attack = attack.ToAttackText(),
                    RulesText = power.RulesText.Items
                        .Add(new("Hit", attack.Hit))
                        .Add(new("Miss", attack.Miss))
                        .Add(new("Special", string.Join(" ", attack.SpecialSentences))),
                };
            }
            else
            {
                return power with
                {
                    RulesText = power.RulesText.Items
                        .Add(new($"{Ordinal(index).Capitalize()} Target", attack.Target))
                        .Add(new($"{Ordinal(index).Capitalize()} Attack", attack.ToAttackText()))
                        .Add(new($"{Ordinal(index).Capitalize()} Hit", attack.Hit))
                        .Add(new($"{Ordinal(index).Capitalize()} Miss", attack.Miss))
                };
            }
        }
    }
}