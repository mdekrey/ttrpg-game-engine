using System;
using System.Threading.Tasks;

namespace GameEngine.Web.Storage;

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

    public static async Task<StorageStatus<T>> UpdateAsync<T>(this ITableStorage<T> storage, TableKey key, Func<T, T> mutator)
    {
        var originalAsyncValue = await storage.LoadAsync(key);
        if (originalAsyncValue is not StorageStatus<T>.Success { Value: T originalValue })
            return new StorageStatus<T>.Failure();

        var newValue = mutator(originalValue);
        await storage.SaveAsync(key, newValue);
        return new StorageStatus<T>.Success(newValue);
    }
}
