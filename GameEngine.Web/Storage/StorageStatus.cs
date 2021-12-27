namespace GameEngine.Web.Storage;

public abstract record StorageStatus<T>
{
    private StorageStatus() { }

    public record Success(T Value) : StorageStatus<T>;
    public record Failure() : StorageStatus<T>;
}
