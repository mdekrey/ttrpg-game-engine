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
        private readonly ICurrentAttacker currentAttacker;
        private readonly ICurrentTarget currentTarget;
        private readonly IMemoryCache memoryCache;

        public ActionFactory(ICurrentAttacker currentAttacker, ICurrentTarget currentTarget, IMemoryCache memoryCache)
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
                { Roll: RollEffectOptions roll } => await FromRollAsync(roll.Method, roll.Resolution),
                { WeaponDamage: WeaponDamageEffectOptions weapon } => new WeaponDamageEffect(),
                _ => throw new NotImplementedException(),
            };
        }

        private IRandomDecisionMaker Build(SerializedDecision method)
        {
            return method switch
            {
                { Attack: AttackRollOptions attack } =>
                    new AttackRoll(currentAttacker, currentTarget)
                    {
                        BaseAttackBonus = Enum.TryParse<Ability>(attack.BaseAttackBonus, out var attackBonus) ? attackBonus : Ability.Strength,
                        Type = Enum.TryParse<AttackRoll.AttackType>(attack.AttackType, out var t) ? t : AttackRoll.AttackType.Physical
                    },
                _ => throw new NotImplementedException(),
            };
        }

        private async Task<RandomizedEffect> FromRollAsync(SerializedDecision method, List<RollEffectResolution> resolution)
        {
            return new RandomizedEffect(
                Build(method),
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
