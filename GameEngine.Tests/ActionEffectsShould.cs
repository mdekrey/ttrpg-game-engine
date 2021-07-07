using GameEngine.Dice;
using GameEngine.Numerics;
using GameEngine.Rules;
using GameEngine.RulesEngine;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace GameEngine.Tests
{
    public class ActionEffectsShould
    {
        private readonly EffectsReducer<double, double> target;
        private readonly ActionFactory actionBuilder;

        public ActionEffectsShould()
        {
            var services = new ServiceCollection();
            services.AddMemoryCache().AddGameEngineRules();
            var serviceProvider = services.BuildServiceProvider();
            target = serviceProvider.GetRequiredService<EffectsReducer<double, double>>();
            actionBuilder = serviceProvider.GetRequiredService<ActionFactory>();
        }

        private static SerializedTarget Deserialize(string json)
        {
            return JsonSerializer.Deserialize<SerializedTarget>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        [Fact]
        public async Task GetAverageDamageForMelee()
        {
            var attackAction = await actionBuilder.BuildAsync(Deserialize(@"{
                ""melee"": {},
                ""effect"": { ""attack"": {
                    ""hit"": { ""weaponDamage"": {} }
                } }
            }"));

            var averageDamage = target.ReduceEffects(attackAction);
            Assert.Equal(4.25, averageDamage);
        }

        [Fact]
        public async Task GetAverageDamageForMeleeTwoAttacks()
        {
            var attackAction = await actionBuilder.BuildAsync(new SerializedTarget
            {
                Melee = new() { TargetCount = 2 },
                Effect = new() { Attack = new() {
                    Hit = new() { WeaponDamage = new() { } }
                } }
            });

            var averageDamage = target.ReduceEffects(attackAction);
            Assert.Equal(8.5, averageDamage);
        }
    }
}
