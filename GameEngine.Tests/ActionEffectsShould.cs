﻿using GameEngine.Dice;
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
        private readonly ServiceProvider serviceProvider;

        public ActionEffectsShould()
        {
            var services = new ServiceCollection();
            services.AddMemoryCache().AddGameEngineRules();
            serviceProvider = services.BuildServiceProvider();
        }

        private static SerializedTarget Deserialize(string json)
        {
            return JsonSerializer.Deserialize<SerializedTarget>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        [Fact]
        public async Task GetAverageDamageForMelee()
        {
            using var scope = serviceProvider.CreateScope();
            var target = scope.ServiceProvider.GetRequiredService<EffectsReducer<double, double>>();
            var actionBuilder = scope.ServiceProvider.GetRequiredService<ActionFactory>();
            var currentTarget = scope.ServiceProvider.GetRequiredService<ICurrentTarget>();
            var currentActor = scope.ServiceProvider.GetRequiredService<ICurrentActor>();

            var attackAction = await actionBuilder.BuildAsync(Deserialize(@"{
                ""melee"": {},
                ""effect"": { ""attack"": {
                    ""hit"": { ""damage"": { ""weapon"": ""[W] + STR"" } }
                } }
            }"));

            var averageDamage = target.ReduceEffects(attackAction);
            Assert.Equal(4.25, averageDamage);
        }

        [Fact]
        public async Task GetAverageDamageForMeleeTwoAttacks()
        {
            using var scope = serviceProvider.CreateScope();
            var target = scope.ServiceProvider.GetRequiredService<EffectsReducer<double, double>>();
            var actionBuilder = scope.ServiceProvider.GetRequiredService<ActionFactory>();

            var attackAction = await actionBuilder.BuildAsync(new SerializedTarget
            {
                Melee = new() { TargetCount = 2 },
                Effect = new() { Attack = new() {
                    Hit = new() { Damage = new() { { DamageType.Weapon, "[W] + STR" } } }
                } }
            });

            var averageDamage = target.ReduceEffects(attackAction);
            Assert.Equal(8.5, averageDamage);
        }
    }
}
