
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameEngine.Web.Storage;
public class GameInMemoryStorage : IGameStorage
{
    private static readonly ConcurrentDictionary<Guid, JToken> data = new ();
    private readonly JsonSerializer jsonSerializer;

    public GameInMemoryStorage(IOptions<GameStorageOptions> gameStorageOptions)
    {
        jsonSerializer = gameStorageOptions.Value.CreateJsonSerializer();
    }

    public async Task SaveAsync(Guid id, object? data)
    {
        await Task.Yield();
        var jsonData = data == null ? JValue.CreateNull() : JToken.FromObject(data, jsonSerializer);
        GameInMemoryStorage.data.AddOrUpdate(id, jsonData, (_, _) => jsonData);
    }

    public async Task<StorageStatus<AsyncProcessed<T>>> UpdateAsync<T>(Guid id, Func<AsyncProcessed<T>, AsyncProcessed<T>> mutator)
    {
        await Task.Yield();
        if (!data.TryGetValue(id, out var original))
            return new StorageStatus<AsyncProcessed<T>>.Failure();

        var newValue = mutator(original.ToObject<AsyncProcessed<T>>(jsonSerializer)!);
        return GameInMemoryStorage.data.TryUpdate(id, newValue == null ? JValue.CreateNull() : JToken.FromObject(newValue, jsonSerializer), original)
            ? new StorageStatus<AsyncProcessed<T>>.Success(newValue!)
            : new StorageStatus<AsyncProcessed<T>>.Failure();
    }

    public Task SaveAsync<T>(Guid id, T data)
    {
        return SaveAsync(id, (object?)data);
    }

    public async Task<StorageStatus<T>> LoadAsync<T>(Guid id)
    {
        await Task.Yield();
        return data.ContainsKey(id)
            ? new StorageStatus<T>.Success(data[id].ToObject<T>(jsonSerializer)!)
            : new StorageStatus<T>.Failure();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        await Task.Yield();
        return data.TryRemove(id, out JToken _);
    }
}
