﻿using GameEngine.Generator.Context;
using GameEngine.Rules;
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
        public static PowerContext ToPowerContext(this ClassPowerProfile profile)
        {
            return new PowerContext(profile.PowerProfile, profile.PowerInfo);
        }
        public static PowerTextBlock ToPowerTextBlock(this PowerContext context)
        {
            var result = new PowerTextBlock(
                Name: "Unknown",
                TypeInfo: $"{context.ToolType.ToText()} Attack {context.Level}",
                FlavorText: null,
                PowerUsage: context.Usage.ToText(),
                Keywords: ImmutableList<string>.Empty.Add(context.ToolType.ToKeyword()),
                ActionType: "Standard Action",
                AttackType: null,
                AttackTypeDetails: null,
                Prerequisite: null,
                Requirement: null,
                Trigger: null,
                Target: null,
                Attack: null,
                RulesText: ImmutableList<RulesText>.Empty,
                AssociatedPower: null
            );

            var attacks = context.GetAttackContexts().Select((attackContext) => attackContext.AttackContext.ToAttackInfo()).ToArray();
            if (attacks.Length > 0)
            {
                result = result.AddAttack(attacks[0], 1);
            }
            result = attacks.Select((attack, index) => (attack, index)).Skip(1).Aggregate(result, (powerBlock, attackBlock) => powerBlock.AddAttack(attackBlock.attack, attackBlock.index + 1));

            result = result with
            {
                RulesText = result.RulesText.AddEffectSentences(context.GetEffectContexts().Select(effectContext => effectContext.EffectContext.ToTargetInfo().PartsToSentence().Capitalize()))
            };
            result = (from mod in context.Modifiers
                      let mutator = mod.GetTextMutator(context)
                      where mutator != null
                      orderby mutator.Priority
                      select mutator.Apply).Aggregate(result, (current, apply) => apply(current));

            return result with
            {
                Keywords = result.Keywords.Items.OrderBy(k => k).ToImmutableList(),
                RulesText = result.RulesText.Items.Where(rule => rule.Text is { Length: > 0 }).ToImmutableList(),
            };
        }

        public static TargetInfo ToTargetInfo(this EffectContext effectContext)
        {
            var result = effectContext.GetDefaultTargetInfo();
            var targetMutator = effectContext.GetTargetInfoMutator();

            result = (from mutator in (from mod in effectContext.Modifiers
                                       let mutator = mod.GetTargetInfoMutator(effectContext)
                                       select mutator).Add(targetMutator)
                      where mutator != null
                      orderby mutator.Priority
                      select mutator.Apply).Aggregate(result, (current, apply) => apply(current));
            return result;
        }

        public static AttackInfo ToAttackInfo(this AttackContext attackContext)
        {
            var targetInfos = attackContext.GetEffectContexts().Select(effectContext => effectContext.EffectContext.ToTargetInfo()).ToArray();
            

            var result = new AttackInfo(
                AttackType: attackContext.GetAttackType(),
                Target: attackContext.GetTargetText(),
                AttackExpression: (GameDiceExpression)attackContext.Ability,
                AttackNotes: attackContext.GetAttackNotes(),
                Defense: DefenseType.ArmorClass,
                HitSentences: targetInfos.Select(effect => effect.PartsToSentence()).ToImmutableList()
                    .AddRange(targetInfos.SelectMany(t => t.AdditionalSentences)),
                MissSentences: ImmutableList<string>.Empty // TODO - miss targets
            );
            

            result = (from mod in attackContext.Modifiers
                      let mutator = mod.GetAttackInfoMutator(attackContext)
                      where mutator != null
                      orderby mutator.Priority
                      select mutator.Apply).Aggregate(result, (current, apply) => apply(current, attackContext.AttackIndex));
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
                    Target = attack.Target.Capitalize(),
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
                        .Add(new($"{Ordinal(index).Capitalize()} Target", attack.Target.Capitalize()))
                        .Add(new($"{Ordinal(index).Capitalize()} Attack", attack.ToAttackText()))
                        .Add(new($"{Ordinal(index).Capitalize()} Hit", attack.Hit))
                        .Add(new($"{Ordinal(index).Capitalize()} Miss", attack.Miss))
                };
            }
        }
    }
}
