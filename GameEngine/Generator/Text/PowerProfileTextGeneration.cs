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
                RulesText = result.RulesText.AddEffectSentences(profile.PowerProfile.Effects.Select(effect => effect.ToTargetInfo(profile.PowerProfile, null).PartsToSentence()))
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
                AttackType: effect.Target.GetAttackType(power, attackIndex: attackIndex),
                AttackNotes: effect.Target.GetAttackNotes(power, attackIndex: attackIndex),
                DamageExpression: null,
                Parts: ImmutableList<string>.Empty,
                AdditionalSentences: ImmutableList<string>.Empty
            );
            var targetMutator = effect.Target.GetTargetInfoMutator(effect, power);

            result = (from mutator in (from mod in effect.Modifiers
                                       let mutator = mod.GetTargetInfoMutator(effect, power)
                                       select mutator).Add(targetMutator)
                      where mutator != null
                      orderby mutator.Priority
                      select mutator.Apply).Aggregate(result, (current, apply) => apply(current));
            return result;
        }

        public static AttackInfo ToAttackInfo(this AttackProfile attack, PowerProfile power, int index)
        {
            var targetInfos = attack.Effects.Select(effect => effect.ToTargetInfo(power, attackIndex: index)).ToArray();
            var targetInfo = targetInfos[0];

            var result = new AttackInfo(
                AttackType: targetInfo.AttackType,
                Target: targetInfo.Target,
                AttackExpression: (GameDiceExpression)attack.Ability,
                AttackNotes: targetInfo.AttackNotes,
                Defense: DefenseType.ArmorClass,
                HitSentences: targetInfos.Skip(1).Select(effect => effect.PartsToSentence()).ToImmutableList()
                    .Insert(0, targetInfo.PartsToSentence())
                    .AddRange(targetInfos.SelectMany(t => t.AdditionalSentences)),
                MissSentences: ImmutableList<string>.Empty // TODO - miss targets
            );
            

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
