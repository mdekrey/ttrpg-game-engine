using System;
using System.Collections.Generic;
using System.Linq;

namespace GameEngine.Generator
{

    public static class PowerBuildingExtensions
    {
        public static TOutput Pipe<TInput, TOutput>(TInput input, Func<TInput, TOutput> transform) => transform(input);
        public static TOutput Pipe<TInput, T1, TOutput>(TInput input, Func<TInput, T1> t1, Func<T1, TOutput> transform) => Pipe(t1(input), transform);
        public static TOutput Pipe<TInput, T1, T2, TOutput>(TInput input, Func<TInput, T1> t1, Func<T1, T2> t2, Func<T2, TOutput> transform) => Pipe(t1(input), t2, transform);
        public static TOutput Pipe<TInput, T1, T2, T3, TOutput>(TInput input, Func<TInput, T1> t1, Func<T1, T2> t2, Func<T2, T3> t3, Func<T3, TOutput> transform) => Pipe(t1(input), t2, t3, transform);
        public static TOutput Pipe<TInput, T1, T2, T3, T4, TOutput>(TInput input, Func<TInput, T1> t1, Func<T1, T2> t2, Func<T2, T3> t3, Func<T3, T4> t4, Func<T4, TOutput> transform) => Pipe(t1(input), t2, t3, t4, transform);
        public static TOutput Pipe<TInput, T1, T2, T3, T4, T5, TOutput>(TInput input, Func<TInput, T1> t1, Func<T1, T2> t2, Func<T2, T3> t3, Func<T3, T4> t4, Func<T4, T5> t5, Func<T5, TOutput> transform) => Pipe(t1(input), t2, t3, t4, t5, transform);
        public static TOutput Pipe<TInput, T1, T2, T3, T4, T5, T6, TOutput>(TInput input, Func<TInput, T1> t1, Func<T1, T2> t2, Func<T2, T3> t3, Func<T3, T4> t4, Func<T4, T5> t5, Func<T5, T6> t6, Func<T6, TOutput> transform) => Pipe(t1(input), t2, t3, t4, t5, t6, transform);
        public static TOutput Pipe<TInput, T1, T2, T3, T4, T5, T6, T7, TOutput>(TInput input, Func<TInput, T1> t1, Func<T1, T2> t2, Func<T2, T3> t3, Func<T3, T4> t4, Func<T4, T5> t5, Func<T5, T6> t6, Func<T6, T7> t7, Func<T7, TOutput> transform) => Pipe(t1(input), t2, t3, t4, t5, t6, t7, transform);

        public static IEnumerable<RandomChances<PowerProfileBuilder>> ToChances(this IEnumerable<PowerProfileBuilder> possibilities, PowerProfileConfig config, bool skipProfile = false) =>
            from possibility in possibilities
            let chances = config.GetChance(possibility, skipProfile)
            where chances > 0
            select new RandomChances<PowerProfileBuilder>(possibility, Chances: (int)chances);

        public static double GetChance(this PowerProfileConfig config, PowerProfileBuilder builder, bool skipProfile = false)
        {
            var (powerToken, modTokens) = GetProfileTokens(builder.Build());
            return (from token in modTokens
                    from weight in (from entry in config.ModifierChances
                                    where token.SelectTokens(entry.Selector).Any()
                                    select entry.Weight).DefaultIfEmpty(0)
                    select weight)
                    .Concat(
                        skipProfile
                            ? Enumerable.Empty<double>()
                            : (from entry in config.PowerChances
                               where powerToken.SelectTokens(entry.Selector).Any()
                               select entry.Weight).DefaultIfEmpty(0)
                    )
                    .Aggregate(1.0, (lhs, rhs) => lhs * rhs);
        }

        public static (Newtonsoft.Json.Linq.JToken powerToken, IEnumerable<Newtonsoft.Json.Linq.JToken> modTokens) GetProfileTokens(this PowerProfile powerProfile)
        {
            var serializer = new Newtonsoft.Json.JsonSerializer()
            {
                Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() }
            };
            return (
                powerToken: Newtonsoft.Json.Linq.JToken.FromObject(powerProfile),
                modTokens: powerProfile.AllModifiers().Select(Newtonsoft.Json.Linq.JToken.FromObject)
            );
        }
    }
}
