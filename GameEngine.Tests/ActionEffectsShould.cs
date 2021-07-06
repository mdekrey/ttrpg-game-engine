using GameEngine.Dice;
using GameEngine.Numerics;
using GameEngine.Rules;
using GameEngine.RulesEngine;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Text;
using Xunit;

namespace GameEngine.Tests
{
    public class ActionEffectsShould
    {
        private readonly EffectsReducer<double, double> target;
        private readonly ICurrentAttacker currentAttacker;
        private readonly ICurrentTarget currentTarget;

        public ActionEffectsShould()
        {
            var services = new ServiceCollection();
            services.AddMemoryCache().AddGameEngineRules();
            var serviceProvider = services.BuildServiceProvider();
            target = serviceProvider.GetRequiredService<EffectsReducer<double, double>>();
            currentAttacker = serviceProvider.GetRequiredService<ICurrentAttacker>();
            currentTarget = serviceProvider.GetRequiredService<ICurrentTarget>();
        }


        [Fact]
        public void GetAverageDamageForMelee()
        {
            var attackAction = new MeleeWeapon
            {
                TargetCount = 1,
                Effect = new RandomizedEffect(
                    new AttackRoll(currentAttacker, currentTarget),
                    new RandomizedEffectList.Builder
                    {
                        { roll => roll >= 0, new DamageEffect(DieCodes.Parse("d8 + 4"), DamageType.Normal) },
                    }
                )
            };

            var averageDamage = target.ReduceEffects(attackAction);
            Assert.Equal(4.25, averageDamage);
        }


        [Fact]
        public void GetAverageDamageForMeleeTwoAttacks()
        {
            var attackAction = new MeleeWeapon
            {
                TargetCount = 2,
                Effect = new RandomizedEffect(
                    new AttackRoll(currentAttacker, currentTarget),
                    new RandomizedEffectList.Builder
                    {
                        { roll => roll >= 0, new DamageEffect(DieCodes.Parse("d8 + 4"), DamageType.Normal) },
                    }
                )
            };

            var averageDamage = target.ReduceEffects(attackAction);
            Assert.Equal(8.5, averageDamage);
        }
    }
}
