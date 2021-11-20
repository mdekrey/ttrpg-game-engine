using GameEngine.Dice;
using GameEngine.Numerics;
using GameEngine.Rules;
using GameEngine.RulesEngine;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Xunit;
using static GameEngine.Generator.ImmutableConstructorExtension;

namespace GameEngine.Tests
{
    public class ActionEffectsShould
    {
        private readonly ServiceProvider serviceProvider;

        public ActionEffectsShould()
        {
            var services = new ServiceCollection();
            services.AddMemoryCache().AddGameEngineRules();
            serviceProvider = services.BuildServiceProvider();
        }
        
        [Fact]
        public void GetAverageDamageForMelee()
        {
            using var scope = serviceProvider.CreateScope();
            var target = scope.ServiceProvider.GetRequiredService<EffectsReducer<double, double>>();

            var attackAction = new MeleeWeapon
            {
                Effect = new AttackRoll
                {
                    Hit = new DamageEffect(Build(new DamageEffectEntry(GameDiceExpression.Parse("[W] + STR"), ImmutableList<DamageType>.Empty))),
                }
            };

            var averageDamage = target.ReduceEffects(attackAction);
            Assert.Equal(4.25, averageDamage);
        }

        [Fact]
        public void GetAverageDamageForMeleeTwoAttacks()
        {
            using var scope = serviceProvider.CreateScope();
            var target = scope.ServiceProvider.GetRequiredService<EffectsReducer<double, double>>();

            var attackAction = new MeleeWeapon
            {
                TargetCount = 2,
                Effect = new AttackRoll
                {
                    Hit = new DamageEffect(Build(new DamageEffectEntry(GameDiceExpression.Parse("[W] + STR"), ImmutableList<DamageType>.Empty))),
                }
            };

            var averageDamage = target.ReduceEffects(attackAction);
            Assert.Equal(8.5, averageDamage);
        }
    }
}
