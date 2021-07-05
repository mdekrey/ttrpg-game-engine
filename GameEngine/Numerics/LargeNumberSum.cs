using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace GameEngine.Numerics
{
    public static class LargeNumberSum
    {
        public static BigInteger Sum(this IEnumerable<BigInteger> orig) =>
            orig.Aggregate((a, b) => a + b);
    }
}
