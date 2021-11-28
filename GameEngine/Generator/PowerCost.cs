namespace GameEngine.Generator
{
    public record PowerCost(double Fixed = 0, double Multiplier = 1)
    {
        public static PowerCost Empty = new PowerCost(Fixed: 0, Multiplier: 1);

        public static PowerCost operator +(PowerCost lhs, PowerCost rhs)
        {
            return new PowerCost(
                Fixed: lhs.Fixed + rhs.Fixed,
                Multiplier: lhs.Multiplier * rhs.Multiplier
            );
        }

        public static PowerCost operator *(PowerCost lhs, double rhs)
        {
            return lhs with { Fixed = lhs.Fixed + rhs };
        }

        public double Apply(double original)
        {
            return (original / Multiplier) - Fixed;
        }
    }
}
