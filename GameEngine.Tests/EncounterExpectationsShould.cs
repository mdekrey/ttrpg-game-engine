using GameEngine.Dice;
using GameEngine.Rules;
using GameEngine.RulesEngine;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace GameEngine.Tests
{
    public record EncounterMonster(CharacterStats Character, double CurrentHitPoints)
    {
        public EncounterMonster(CharacterStats Character) : this(Character, Character.MaxHitPoints) { }
    }

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

            var pc = CharacterStats.Default;
            //based on Goblin Warrior
            var goblin = MonsterRoleTemplate.Skirmisher.GetStatsFor(level, new Rules.CharacterAbilities(2, 1, 3, -1, 1, -1));

            var pcBonusDamagePerRound = 
                await ParseToDamage(scope, pc, goblin, @"{ ""target"": { ""meleeWeapon"": {}, ""effect"": { ""attack"": { ""hit"": { ""damage"": [ { ""types"": [""normal""], ""amount"": ""2[W] + STR"" } ] } } } } }");
            var pcStandardDamagePerRound = 
                await ParseToDamage(scope, pc, goblin, @"{ ""target"": { ""meleeWeapon"": {}, ""effect"": { ""attack"": { ""hit"": { ""damage"": [ { ""types"": [""normal""], ""amount"": ""[W] + STR"" } ] } } } } }");
            var damage = GenerateDamagePerAction(new[] { (1, pcBonusDamagePerRound) }, pcStandardDamagePerRound);

            var healthRemainingPerRound = SimulateCombat(damage, 1, goblin).ToArray();
            Assert.Equal(6, healthRemainingPerRound.Length);
        }

        [Fact]
        public async Task MeetAverageEncounterLengthForSingleMonsterAsRanger()
        {
            using var scope = serviceProvider.CreateScope();
            const int level = 1;

            var pc = CharacterStats.Default;
            //based on Goblin Warrior
            var goblin = MonsterRoleTemplate.Skirmisher.GetStatsFor(level, new Rules.CharacterAbilities(2, 1, 3, -1, 1, -1));

            var pcBonusDamagePerRound =
                await ParseToDamage(scope, pc, goblin, @"{ ""target"": { ""meleeWeapon"": {}, ""effect"": { ""attack"": { ""hit"": { ""damage"": [ { ""types"": [""normal""], ""amount"": ""2[W] + d6 + STR"" } ] } } } } }");
            var pcStandardDamagePerRound =
                await ParseToDamage(scope, pc, goblin, @"{ ""all"": [
                    { ""target"": { ""meleeWeapon"": {}, ""effect"": { ""attack"": { ""bonus"": ""1"", ""hit"": { ""damage"": [ { ""types"": [""normal""], ""amount"": ""[W] + d6"" } ] } } } } },
                    { ""target"": { ""meleeWeapon"": {}, ""effect"": { ""attack"": { ""bonus"": ""1"", ""hit"": { ""damage"": [ { ""types"": [""normal""], ""amount"": ""[W] - 1"" } ] } } } } }
                ] }");
            var damage = GenerateDamagePerAction(new[] { (1, pcBonusDamagePerRound) }, pcStandardDamagePerRound);

            var healthRemainingPerRound = SimulateCombat(damage, 1, goblin).ToArray();
            Assert.Equal(4, healthRemainingPerRound.Length);
        }

        [Fact]
        public async Task MeetAverageEncounterLengthForSingleMonsterAsRangerWithCarefulAttack()
        {
            using var scope = serviceProvider.CreateScope();
            const int level = 1;

            var pc = CharacterStats.Default;
            //based on Goblin Warrior
            var goblin = MonsterRoleTemplate.Skirmisher.GetStatsFor(level, new Rules.CharacterAbilities(2, 1, 3, -1, 1, -1));

            var pcBonusDamagePerRound =
                await ParseToDamage(scope, pc, goblin, @"{ ""target"": { ""meleeWeapon"": {}, ""effect"": { ""attack"": { ""hit"": { ""damage"": [ { ""types"": [""normal""], ""amount"": ""2[W] + d6 + STR"" } ] } } } } }");
            var pcStandardDamagePerRound =
                await ParseToDamage(scope, pc, goblin, @"{ ""target"": { ""meleeWeapon"": {}, ""effect"": { ""attack"": { ""bonus"": ""3"", ""hit"": { ""damage"": [ { ""types"": [""normal""], ""amount"": ""[W] + d6 + STR"" } ] } } } } }");
            var damage = GenerateDamagePerAction(new[] { (1, pcBonusDamagePerRound) }, pcStandardDamagePerRound);

            var healthRemainingPerRound = SimulateCombat(damage, 1, goblin).ToArray();
            Assert.Equal(4, healthRemainingPerRound.Length);
        }

        [Fact]
        public async Task MeetAverageEncounterLengthForPartyOf4()
        {
            using var scope = serviceProvider.CreateScope();
            const int level = 1;

            var pc = CharacterStats.Default;
            //based on Goblin Warrior
            var goblin = MonsterRoleTemplate.Skirmisher.GetStatsFor(level, new Rules.CharacterAbilities(2, 1, 3, -1, 1, -1));

            var pcBonusDamagePerRound = 
                await ParseToDamage(scope, pc, goblin, @"{ ""target"": { ""meleeWeapon"": { ""targetCount"": 2 }, ""effect"": { ""attack"": { ""hit"": { ""damage"": [ { ""types"": [""normal""], ""amount"": ""[W] + STR"" } ] } } } } }");
            var pcStandardDamagePerRound = 
                await ParseToDamage(scope, pc, goblin, @"{ ""target"": { ""meleeWeapon"": {}, ""effect"": { ""attack"": { ""hit"": { ""damage"": [ { ""types"": [""normal""], ""amount"": ""[W] + STR"" } ] } } } } }");
            var damage = GenerateDamagePerAction(new[] { (4, pcBonusDamagePerRound) }, pcStandardDamagePerRound);

            var healthRemainingPerRound = SimulateCombat(damage, 4, goblin, goblin, goblin, goblin).ToArray();
            Assert.Equal(7, healthRemainingPerRound.Length);
        }

        private IEnumerable<ImmutableList<EncounterMonster>> SimulateCombat(IEnumerable<double> damage, int attacksPerRound, params CharacterStats[] opponents)
        {
            var current = opponents.Select(opp => new EncounterMonster(opp, opp.MaxHitPoints)).ToList();
            using var damageEnumerator = damage.GetEnumerator();
            while (current.Any(entry => entry.CurrentHitPoints > 0))
            {
                for (var i = 0; i < attacksPerRound; i++)
                {
                    damageEnumerator.MoveNext();
                    var index = current.FindIndex(m => m.CurrentHitPoints > 0);
                    if (index == -1)
                        break;
                    current[index] = current[index] with { CurrentHitPoints = current[index].CurrentHitPoints - damageEnumerator.Current };
                }
                yield return current.ToImmutableList();
            }
        }

        private IEnumerable<double> GenerateDamagePerAction((int times, double damage)[] limitedDamage, params double[] atWillDamage)
        {
            foreach (var entry in limitedDamage.OrderByDescending(v => v.damage))
            {
                for (var i = 0; i < entry.times; i++)
                    yield return entry.damage;
            }
            while (true)
            {
                foreach (var entry in atWillDamage)
                    yield return entry;
            }
        }

        private static async Task<double> ParseToDamage(IServiceScope scope, CharacterStats actor, CharacterStats target, string action)
        {
            var damageCalculator = scope.ServiceProvider.GetRequiredService<IEffectsReducer<double>>();
            var actionBuilder = scope.ServiceProvider.GetRequiredService<ActionFactory>();
            var currentTarget = scope.ServiceProvider.GetRequiredService<CurrentTarget>();
            var currentActor = scope.ServiceProvider.GetRequiredService<CurrentActor>();
            currentTarget.Current = target;
            currentActor.Current = actor;

            var serializedEffect = Deserialize(action);
            return damageCalculator!.ReduceEffects(await actionBuilder!.BuildAsync(serializedEffect));
        }

        private static SerializedEffect Deserialize(string json)
        {
            return JsonSerializer.Deserialize<SerializedEffect>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true, Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() } })!;
        }
    }
}
