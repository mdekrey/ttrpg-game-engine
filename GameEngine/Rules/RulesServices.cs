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

                return new EffectsReducer<double, double>(allEffects => allEffects.Select(e => e.Probability * e.MappedEffect).Sum())
                    .AddTarget<MeleeWeapon>(attack => attack.TargetCount)
                    .AddDecision<DieCodeRandomDecisionMaker>(dieCode => permutations.Permutations(dieCode.Dice))
                    .AddDecision<AttackRoll>(attackRoll => new (Enumerable.Repeat<BigInteger>(1, 2).ToImmutableList(), -1))
                    .AddEffect<DamageEffect>(outcome => outcome.Damage.Mean())
                    .AddEffect<WeaponDamageEffect>(outcome => 8.5) // TODO
                    .AddEffect<NoEffect>(outcome => 0);
            });

            services.AddScoped<ICurrentAttacker, CurrentAttacker>();
            services.AddScoped<ICurrentTarget, CurrentTarget>();
            services.AddTransient<ActionFactory>();

            return services;
        }
    }
}
