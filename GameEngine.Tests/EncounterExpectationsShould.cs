using GameEngine.Dice;
using GameEngine.Rules;
using GameEngine.RulesEngine;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace GameEngine.Tests
{
    public class EncounterExpectationsShould
    {
        private readonly ServiceProvider serviceProvider;

        public EncounterExpectationsShould()
        {
            var services = new ServiceCollection();
            services.AddMemoryCache().AddGameEngineRules();
            serviceProvider = services.BuildServiceProvider();
        }

        [Fact]
        public async Task MeetAverageEncounterLengthForSingleMonster()
        {
            using var scope = serviceProvider.CreateScope();

            const int level = 1;

            var pc = CharacterStats.Default; // TODO
            //based on Goblin Warrior
            var goblin = MonsterRoleTemplate.Skirmisher.GetStatsFor(level, new Rules.CharacterAbilities(2, 1, 3, -1, 1, -1));

            //var pcBonusDamagePerRound = DieCodes.Parse(CombatExpectations.limitedMonsterDamage[level / 3].High).Mean() * chanceToHit;
            var pcBonusDamagePerRound = await ParseToDamage(pc, goblin, @"{
                ""melee"": {},
                ""effect"": { ""attack"": {
                    ""hit"": { ""damage"": { ""amount"": ""2[W] + STR"" } }
                } }
            }");
            //var pcStandardDamagePerRound = DieCodes.Parse(CombatExpectations.standardMonsterDamage[level / 3].High).Mean() * chanceToHit;
            var pcStandardDamagePerRound = await ParseToDamage(pc, goblin, @"{
                ""melee"": {},
                ""effect"": { ""attack"": {
                    ""hit"": { ""damage"": { ""amount"": ""[W] + STR"" } }
                } }
            }");
            var damage = GenerateDamagePerAction(new[] { (1, pcBonusDamagePerRound) }, pcStandardDamagePerRound);

            var monsterDamage = await ParseToDamage(goblin, pc, @"{
                ""melee"": {},
                ""effect"": { ""attack"": {
                    ""hit"": { ""damage"": { ""amount"": ""[W] + STR"" } }
                } }
            }");

            var healthRemainingPerRound = damage.Scan((double)goblin.MaxHitPoints, (hp, damage) => hp - damage).TakeWhile(hp => hp > 0).Concat(new[] { 0.0 }).ToArray();
            Assert.Equal(6, healthRemainingPerRound.Length);

            async Task<double> ParseToDamage(CharacterStats actor, CharacterStats target, string action)
            {
                var damageCalculator = scope.ServiceProvider.GetRequiredService<IEffectsReducer<double>>();
                var actionBuilder = scope.ServiceProvider.GetRequiredService<ActionFactory>();
                var currentTarget = scope.ServiceProvider.GetRequiredService<CurrentTarget>();
                var currentActor = scope.ServiceProvider.GetRequiredService<CurrentActor>();
                currentTarget.Current = target;
                currentActor.Current = actor;

                return damageCalculator!.ReduceEffects(await actionBuilder!.BuildAsync(Deserialize(action)));
            }
        }

        private IEnumerable<double> GenerateDamagePerAction((int times, double damage)[] limitedDamage, params double[] atWillDamage)
        {
            foreach (var entry in limitedDamage.OrderByDescending(v => v.damage))
            {
                for (var i = 0; i < entry.times; i++)
                    yield return entry.damage;
            }
            while(true)
            {
                foreach (var entry in atWillDamage)
                    yield return entry;
            }
        }

        private static SerializedTarget Deserialize(string json)
        {
            return JsonSerializer.Deserialize<SerializedTarget>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }

    public static class ScanExtensions
    {
        public static IEnumerable<TResult> Scan<TInput, TResult>(this IEnumerable<TInput> inputs, TResult seed, Func<TResult, TInput, TResult> reducer)
        {
            TResult current = seed;
            foreach (var next in inputs)
            {
                current = reducer(current, next);
                yield return current;
            }
        }
    }
}
