using System;
using System.Threading.Tasks;

namespace GameEngine.Web.Storage;

public interface IGameStorage
{
    Task<bool> DeleteAsync(Guid id);
    Task<StorageStatus<T>> LoadAsync<T>(Guid id);
    Task SaveAsync(Guid id, object? data);
    Task<StorageStatus<AsyncProcessed<T>>> UpdateAsync<T>(Guid id, Func<AsyncProcessed<T>, AsyncProcessed<T>> mutator);

    Task SaveAsync<T>(Guid id, T data)
    {
        return SaveAsync(id, (object?)data);
    }
}

public abstract record StorageStatus<T>
{
    private StorageStatus() { }

    public record Success(T Value) : StorageStatus<T>;
    public record Failure() : StorageStatus<T>;
}
