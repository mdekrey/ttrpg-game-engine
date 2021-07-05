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
    public class ActionOutcomesShould
    {
        private readonly OutcomesReducer<double, double> target;

        public ActionOutcomesShould()
        {
            var services = new ServiceCollection();
            services.AddMemoryCache();
            var serviceProvider = services.BuildServiceProvider();

            var memoryCache = serviceProvider.GetService<IMemoryCache>();
            var permutations = new DicePermutations(memoryCache);

            target = new OutcomesReducer<double, double>(allOutcomes => allOutcomes.Select(e => e.probability * e.mappedOutcome).Sum())
                .AddDecision<DieCodeRandomDecisionMaker>(dieCode => permutations.Permutations(dieCode.Dice))
                .AddOutcome<DamageOutcome>(outcome => outcome.Damage.Mean())
                .AddOutcome<EmptyOutcome>(outcome => 0);
        }


        [Fact]
        public void GetAverageDamage()
        {
            var attackAction = new RandomizedDecision(
                new DieCodeRandomDecisionMaker(DieCodes.Parse("d20 + 5")),
                new Outcomes.Builder
                {
                    { roll => roll >= 16, new DamageOutcome(DieCodes.Parse("d8 + 4"), DamageType.Normal) },
                }
            );

            var averageDamage = target.ReduceOutcomes(attackAction);
            Assert.Equal(4.25, averageDamage);
        }
    }
}
