namespace GameEngine.Combining
{
    public abstract record CombineResult<T>()
    {
        public static readonly CannotCombine Cannot = new CannotCombine();
        public static CombineResult<T> Use(T single) => new CombineToOne(single);

        public record CannotCombine() : CombineResult<T>();
        public record CombineToOne(T Result) : CombineResult<T>();
        public record Simplify(T Original, T Other) : CombineResult<T>();
    }
}
