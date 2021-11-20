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
using static GameEngine.Generator.ImmutableConstructorExtension;

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
        public void MeetAverageEncounterLengthForSingleMonster()
        {
            using var scope = serviceProvider.CreateScope();
            const int level = 1;

            var pc = CharacterStats.Default;
            //based on Goblin Warrior
            var goblin = MonsterRoleTemplate.Skirmisher.GetStatsFor(level, new Rules.CharacterAbilities(2, 1, 3, -1, 1, -1));

            var pcBonusDamagePerRound = 
                ToDamage(scope, pc, goblin, new MeleeWeapon
                {
                    Effect = new AttackRoll
                    {
                        Hit = new DamageEffect(Build(new DamageEffectEntry(GameDiceExpression.Parse("2[W] + STR"), ImmutableList<DamageType>.Empty))),
                    }
                });
            var pcStandardDamagePerRound = 
                ToDamage(scope, pc, goblin, new MeleeWeapon
                {
                    Effect = new AttackRoll
                    {
                        Hit = new DamageEffect(Build(new DamageEffectEntry(GameDiceExpression.Parse("[W] + STR"), ImmutableList<DamageType>.Empty))),
                    }
                });
            var damage = GenerateDamagePerAction(new[] { (1, pcBonusDamagePerRound) }, pcStandardDamagePerRound);

            var healthRemainingPerRound = SimulateCombat(damage, 1, goblin).ToArray();
            Assert.Equal(6, healthRemainingPerRound.Length);
        }

        [Fact]
        public void MeetAverageEncounterLengthForSingleMonsterAsRanger()
        {
            using var scope = serviceProvider.CreateScope();
            const int level = 1;

            var pc = CharacterStats.Default;
            //based on Goblin Warrior
            var goblin = MonsterRoleTemplate.Skirmisher.GetStatsFor(level, new Rules.CharacterAbilities(2, 1, 3, -1, 1, -1));

            var pcBonusDamagePerRound =
                ToDamage(scope, pc, goblin, new MeleeWeapon
                {
                    Effect = new AttackRoll
                    {
                        Hit = new DamageEffect(Build(new DamageEffectEntry(GameDiceExpression.Parse("2[W] + d6 + STR"), ImmutableList<DamageType>.Empty))),
                    }
                });
            var pcStandardDamagePerRound =
                ToDamage(scope, pc, goblin, new MeleeWeapon
                {
                    Effect = new AllEffects(Build<IEffect>(
                        new AttackRoll
                        {
                            Bonus = 1,
                            Hit = new DamageEffect(Build(new DamageEffectEntry(GameDiceExpression.Parse("[W] + d6"), ImmutableList<DamageType>.Empty))),
                        },
                        new AttackRoll
                        {
                            Bonus = 1,
                            Hit = new DamageEffect(Build(new DamageEffectEntry(GameDiceExpression.Parse("[W] - 1"), ImmutableList<DamageType>.Empty))),
                        }
                    ))
                });
            var damage = GenerateDamagePerAction(new[] { (1, pcBonusDamagePerRound) }, pcStandardDamagePerRound);

            var healthRemainingPerRound = SimulateCombat(damage, 1, goblin).ToArray();
            Assert.Equal(4, healthRemainingPerRound.Length);
        }

        [Fact]
        public void MeetAverageEncounterLengthForSingleMonsterAsRangerWithCarefulAttack()
        {
            using var scope = serviceProvider.CreateScope();
            const int level = 1;

            var pc = CharacterStats.Default;
            //based on Goblin Warrior
            var goblin = MonsterRoleTemplate.Skirmisher.GetStatsFor(level, new Rules.CharacterAbilities(2, 1, 3, -1, 1, -1));

            var pcBonusDamagePerRound =
                ToDamage(scope, pc, goblin, new MeleeWeapon
                {
                    Effect = new AttackRoll
                    {
                        Hit = new DamageEffect(Build(new DamageEffectEntry(GameDiceExpression.Parse("2[W] + d6 + STR"), ImmutableList<DamageType>.Empty))),
                    }
                });
            var pcStandardDamagePerRound =
                ToDamage(scope, pc, goblin, new MeleeWeapon
                {
                    Effect = new AttackRoll
                    {
                        Bonus = 3,
                        Hit = new DamageEffect(Build(new DamageEffectEntry(GameDiceExpression.Parse("[W] + d6 + STR"), ImmutableList<DamageType>.Empty))),
                    }
                });
            var damage = GenerateDamagePerAction(new[] { (1, pcBonusDamagePerRound) }, pcStandardDamagePerRound);

            var healthRemainingPerRound = SimulateCombat(damage, 1, goblin).ToArray();
            Assert.Equal(4, healthRemainingPerRound.Length);
        }

        [Fact]
        public void MeetAverageEncounterLengthForPartyOf4()
        {
            using var scope = serviceProvider.CreateScope();
            const int level = 1;

            var pc = CharacterStats.Default;
            //based on Goblin Warrior
            var goblin = MonsterRoleTemplate.Skirmisher.GetStatsFor(level, new Rules.CharacterAbilities(2, 1, 3, -1, 1, -1));

            var pcBonusDamagePerRound = 
                ToDamage(scope, pc, goblin, new MeleeWeapon
                {
                    TargetCount = 2,
                    Effect = new AttackRoll
                    {
                        Hit = new DamageEffect(Build(new DamageEffectEntry(GameDiceExpression.Parse("[W] + STR"), ImmutableList<DamageType>.Empty))),
                    }
                });
            var pcStandardDamagePerRound = 
                ToDamage(scope, pc, goblin, new MeleeWeapon
                {
                    Effect = new AttackRoll
                    {
                        Hit = new DamageEffect(Build(new DamageEffectEntry(GameDiceExpression.Parse("[W] + STR"), ImmutableList<DamageType>.Empty))),
                    }
                });
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

        private static double ToDamage(IServiceScope scope, CharacterStats actor, CharacterStats target, IEffect action)
        {
            var damageCalculator = scope.ServiceProvider.GetRequiredService<IEffectsReducer<double>>();
            var currentTarget = scope.ServiceProvider.GetRequiredService<CurrentTarget>();
            var currentActor = scope.ServiceProvider.GetRequiredService<CurrentActor>();
            currentTarget.Current = target;
            currentActor.Current = actor;

            return damageCalculator!.ReduceEffects(action);
        }
    }
}
