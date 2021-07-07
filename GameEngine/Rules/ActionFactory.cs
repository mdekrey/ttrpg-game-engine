using GameEngine.Dice;
using GameEngine.RulesEngine;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Rules
{
    public class ActionFactory
    {
        private readonly ICurrentActor currentAttacker;
        private readonly ICurrentTarget currentTarget;
        private readonly IMemoryCache memoryCache;

        public ActionFactory(ICurrentActor currentAttacker, ICurrentTarget currentTarget, IMemoryCache memoryCache)
        {
            this.currentAttacker = currentAttacker ?? throw new ArgumentNullException(nameof(currentAttacker));
            this.currentTarget = currentTarget ?? throw new ArgumentNullException(nameof(currentTarget));
            this.memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        }

        public async Task<ITargetSelection> BuildAsync(SerializedTarget target)
        {
            return target switch
            {
                { Melee: MeleeWeaponOptions melee } => new MeleeWeapon { AdditionalReach = melee.AdditionalReach, TargetCount = melee.TargetCount, Effect = await BuildAsync(target.Effect) },
                _ => throw new NotImplementedException(),
            };
        }

        private async Task<IEffect> BuildAsync(SerializedEffect effect)
        {
            return effect switch
            {
                { Damage: DamageEffectOptions damage } => new DamageEffect(DieCodes.Parse(damage.DieCodes), Enum.TryParse<DamageType>(damage.DamageType, out var result) ? result : DamageType.Normal),
                { All: List<SerializedEffect> effects } => new AllEffects((await Task.WhenAll(effects.Select(BuildAsync))).ToImmutableList()),
                { Randomized: RandomizedOptions roll } => await FromRollAsync(roll.Dice, roll.Resolution),
                { WeaponDamage: WeaponDamageEffectOptions weapon } => new WeaponDamageEffect(),
                { Attack: AttackRollOptions attack } => 
                    new AttackRoll(currentAttacker, currentTarget)
                    {
                        BaseAttackBonus = Enum.TryParse<Ability>(attack.BaseAttackBonus, out var attackBonus) ? attackBonus : Ability.Strength,
                        Type = Enum.TryParse<AttackRoll.AttackType>(attack.AttackType, out var t) ? t : AttackRoll.AttackType.Physical,
                        Hit = attack.Hit == null ? null : await BuildAsync(attack.Hit),
                        Miss = attack.Miss == null ? null : await BuildAsync(attack.Miss),
                        Effect = attack.Effect == null ? null : await BuildAsync(attack.Effect),
                    },
                _ => throw new NotImplementedException(),
            };
        }

        private async Task<DieCodeRandomizedEffect> FromRollAsync(string dice, List<RollEffectResolution> resolution)
        {
            return new DieCodeRandomizedEffect(
                DieCodes.Parse(dice),
                new RandomizedEffectList(
                    (await Task.WhenAll(
                        resolution.Select(async entry => new RandomizedEffectListEntry(
                            await BuildPredicateAsync(entry.Expression),
                            await BuildAsync(entry.Effect)
                        ))
                    )).ToImmutableList()
                )
            );
        }

        private Task<Predicate<int>> BuildPredicateAsync(string expression)
        {
            return memoryCache.GetOrCreateAsync(
                $"{nameof(ActionFactory)}-{nameof(BuildPredicateAsync)}-{expression}",
                _ => CSharpScript.EvaluateAsync<Predicate<int>>(expression)
            );
        }
    }

}
