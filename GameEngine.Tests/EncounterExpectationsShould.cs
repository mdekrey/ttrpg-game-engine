using GameEngine.Dice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace GameEngine.Tests
{
    public class EncounterExpectationsShould
    {
        [Fact]
        public void MeetAverageEncounterLengthForSingleMonster()
        {
            const int level = 1;
            const double chanceToHit = 0.5;
            var pcBonusDamagePerRound = DieCodes.Parse(CombatExpectations.limitedDamage[level / 3].High).Mean() * chanceToHit;
            var pcStandardDamagePerRound = DieCodes.Parse(CombatExpectations.standardDamage[level / 3].High).Mean() * chanceToHit;

            var damage = GenerateDamagePerAction(new[] { (1, pcBonusDamagePerRound) }, pcStandardDamagePerRound);
            
            //based on Goblin Warrior
            const int monsterHp = 29;
            var damagePerRound = 
                (DieCodes.Parse(CombatExpectations.standardDamage[level / 3].High).Mean() * 0.5 
                + DieCodes.Parse(CombatExpectations.standardDamage[level / 3].Medium).Mean() * 0.5)
                * chanceToHit;

            var healthRemainingPerRound = damage.Scan((double)monsterHp, (hp, damage) => hp - damage).TakeWhile(hp => hp > 0).ToArray();
            Assert.Equal(5, healthRemainingPerRound.Length);
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
