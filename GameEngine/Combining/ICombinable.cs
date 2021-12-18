namespace GameEngine.Combining
{
    public interface ICombinable<T>
    {
        CombineResult<T> Combine(T other);
    }
}
