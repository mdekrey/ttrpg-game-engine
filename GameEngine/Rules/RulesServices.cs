using GameEngine.Dice;
using GameEngine.RulesEngine;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;

namespace GameEngine.Rules
{
    public static class RulesServices
    {
        public static IServiceCollection AddGameEngineRules(this IServiceCollection services)
        {
            services.AddSingleton<DicePermutations>();

            services.AddSingleton(sp =>
            {
                var permutator = sp.GetRequiredService<DicePermutations>();
                var actor = sp.GetRequiredService<ICurrentActor>();
                var target = sp.GetRequiredService<ICurrentTarget>();

                var result = new EffectsReducer<double, double>(allEffects => allEffects.Select(e => e.Probability * e.MappedEffect).Sum());
                return result
                    .AddTarget<MeleeWeapon>(attack => attack.TargetCount)
                    .AddEffect<DieCodeRandomizedEffect>(dieCode =>
                    {
                        var permutations = permutator.Permutations(dieCode.Dice);
                        return MapEffects(result, permutations, dieCode.DecisionEffects.Entries);
                    })
                    .AddEffect<AttackRoll>(attack =>
                    {
                        // TODO - odds based on actor and target
                        // TODO - saves
                        var permutations = permutator.Permutations(DieCodes.Parse("d20 + 5 - 16"));
                        var effects = new RandomizedEffectList.Builder();
                        if (attack.Hit != null)
                            effects.Add(roll => roll >= 0, attack.Hit);
                        if (attack.Miss != null)
                            effects.Add(roll => roll < 0, attack.Miss);
                        if (attack.Effect != null)
                            effects.Add(roll => true, attack.Effect);
                        var entries = ((RandomizedEffectList)effects).Entries;
                        return MapEffects(result, permutations, entries);
                    })
                    .AddEffect<DamageEffect>(outcome => outcome.Damage.Mean())
                    .AddEffect<WeaponDamageEffect>(outcome => CombatExpectations.averagePrimaryWeaponDamage + CombatExpectations.ExpectedPrimaryAbilityModifier(actor.Current.Level)) // TODO - use actual info
                    .AddEffect<NoEffect>(outcome => 0);
            });

            services.AddScoped<ICurrentActor, CurrentActor>();
            services.AddScoped<ICurrentTarget, CurrentTarget>();
            services.AddTransient<ActionFactory>();

            return services;
        }

        private static IEnumerable<EffectsReducer<double, double>.MappedProbability> MapEffects(EffectsReducer<double, double> result, Numerics.PermutationsResult permutations, ImmutableList<RandomizedEffectListEntry> entries)
        {
            return from entry in entries
                   let probability = (double)permutations.Odds(entry.Applies)
                   from mappedEffect in result.MapEffect(entry.Effect)
                   select mappedEffect with { Probability = probability * mappedEffect.Probability };
        }
    }
}
