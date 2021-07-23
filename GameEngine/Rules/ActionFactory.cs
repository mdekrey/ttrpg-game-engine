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
            var effect = await BuildAsync(target.Effect);
            return target switch
            {
                { MeleeWeapon: MeleeWeaponOptions melee } => new MeleeWeapon { AdditionalReach = melee.AdditionalReach, TargetCount = melee.TargetCount, Offhand = melee.Offhand, Effect = effect },
                { RangedWeapon: RangedWeaponOptions _ } => new RangedWeapon { },
                _ => throw new NotImplementedException(),
            };
        }

        public async Task<IEffect> BuildAsync(SerializedEffect effect)
        {
            var effects = (await BuildEffects(effect)).ToList();

            return effects.Count == 1
                ? effects[0]
                : new AllEffects(effects.ToImmutableList());
        }

        private async Task<IEnumerable<IEffect>> BuildEffects(SerializedEffect effect)
        {
            var effects = new List<IEffect>();

            if (effect is { All: IEnumerable<SerializedEffect> multiple })
                effects.AddRange((await Task.WhenAll(multiple.Select(BuildEffects))).SelectMany(e => e));
            if (effect is { Damage: DamageEffectOptions damage })
                effects.Add(new DamageEffect(damage.ToImmutableDictionary(kvp => kvp.Key, kvp => GameDiceExpression.Parse(kvp.Value))));
            if (effect is { Randomized: RandomizedOptions roll })
                effects.Add(await FromRollAsync(roll.Dice, roll.Resolution));
            if (effect is { Attack: AttackRollOptions attack })
                effects.Add(new AttackRoll(currentAttacker, currentTarget)
                {
                    Kind = Enum.TryParse<Ability>(attack.Kind, out var attackBonus) ? attackBonus : Ability.Strength,
                    Bonus = attack.Bonus,
                    Defense = Enum.TryParse<AttackRoll.AttackType>(attack.Defense, out var t) ? t : AttackRoll.AttackType.ArmorClass,
                    Hit = attack.Hit == null ? null : await BuildAsync(attack.Hit),
                    Miss = attack.Miss == null ? null : await BuildAsync(attack.Miss),
                    Effect = attack.Effect == null ? null : await BuildAsync(attack.Effect),
                });
            if (effect is { Target: SerializedTarget target })
                effects.Add(await BuildAsync(target));
            if (effect is { HalfDamage: true })
                effects.Add(new HalfDamageEffect());

            if (effect is { Power: SerializedPower power })
                throw new NotImplementedException(); // TODO

            // TODO - is there a way to make sure we handle all cases?
            return effects;
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
