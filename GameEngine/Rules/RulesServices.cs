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

            services.AddScoped(sp =>
            {
                // scoped because it uses scoped services (current actor/current target)
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
                        // TODO - saves
                        var modifier = actor.Current.Abilities[attack.BaseAttackBonus] + CombatExpectations.ExpectedProficiencyModifier(actor.Current.Level) + attack.Bonus
                            - target.Current.ArmorClass;
                        var permutations = permutator.Permutations(new DieCode(1, 20));
                        permutations += modifier;
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
                    .AddEffect<DamageEffect>(outcome => outcome.Damage.With(CombatExpectations.averagePrimaryWeaponDieCode, actor.Current.Abilities).Mean())
                    .AddEffect<NoEffect>(outcome => 0);
            });
            services.AddTransient<IEffectsReducer<double>>(sp => sp.GetRequiredService<EffectsReducer<double, double>>());

            services.AddScoped<CurrentActor>();
            services.AddScoped<CurrentTarget>();
            services.AddTransient<ICurrentActor>(sp => sp.GetRequiredService<CurrentActor>());
            services.AddTransient<ICurrentTarget>(sp => sp.GetRequiredService<CurrentTarget>());
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
