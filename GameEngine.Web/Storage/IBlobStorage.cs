using System;
using System.Threading.Tasks;

namespace GameEngine.Web.Storage;

public interface IBlobStorage<T>
{
    Task<bool> DeleteAsync(Guid id);
    Task<StorageStatus<T>> LoadAsync(Guid id);
    Task SaveAsync(Guid id, T data);
}

public static class GameStorageExtensions
{
    public static async Task<StorageStatus<AsyncProcessed<T>>> UpdateAsync<T>(this IBlobStorage<AsyncProcessed<T>> storage, Guid id, Func<AsyncProcessed<T>, AsyncProcessed<T>> mutator)
    {
        var originalAsyncValue = await storage.LoadAsync(id);
        if (originalAsyncValue is not StorageStatus<AsyncProcessed<T>>.Success { Value: AsyncProcessed<T> originalValue })
            return new StorageStatus<AsyncProcessed<T>>.Failure();

        var newValue = mutator(originalValue);
        await storage.SaveAsync(id, newValue);
        return new StorageStatus<AsyncProcessed<T>>.Success(newValue);
    }

}

public abstract record StorageStatus<T>
{
    private StorageStatus() { }

    public record Success(T Value) : StorageStatus<T>;
    public record Failure() : StorageStatus<T>;
}
