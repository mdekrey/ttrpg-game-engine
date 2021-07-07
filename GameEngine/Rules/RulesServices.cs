using GameEngine.Dice;
using GameEngine.RulesEngine;
using Microsoft.Extensions.DependencyInjection;
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
                var permutations = sp.GetRequiredService<DicePermutations>();
                var actor = sp.GetRequiredService<ICurrentActor>();
                var target = sp.GetRequiredService<ICurrentTarget>();

                return new EffectsReducer<double, double>(allEffects => allEffects.Select(e => e.Probability * e.MappedEffect).Sum())
                    .AddTarget<MeleeWeapon>(attack => attack.TargetCount)
                    .AddDecision<DieCodeRandomDecisionMaker>(dieCode => permutations.Permutations(dieCode.Dice))
                    .AddDecision<AttackRoll>(attackRoll => permutations.Permutations(DieCodes.Parse("d20 + 5 - 16")))
                    .AddEffect<DamageEffect>(outcome => outcome.Damage.Mean())
                    .AddEffect<WeaponDamageEffect>(outcome => CombatExpectations.averagePrimaryWeaponDamage + CombatExpectations.ExpectedPrimaryAbilityModifier(actor.Current.Level)) // TODO - use actual info
                    .AddEffect<NoEffect>(outcome => 0);
            });

            services.AddScoped<ICurrentActor, CurrentActor>();
            services.AddScoped<ICurrentTarget, CurrentTarget>();
            services.AddTransient<ActionFactory>();

            return services;
        }
    }
}
