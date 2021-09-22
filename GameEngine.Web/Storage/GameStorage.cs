
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameEngine.Web.Storage;
public class GameStorage
{
    private static readonly ConcurrentDictionary<Guid, JToken> data = new ();
    private readonly JsonSerializer jsonSerializer;

    public abstract record Status<T>()
    {
        public record Success(T Value) : Status<T>;
        public record Failure() : Status<T>;
    }

    public GameStorage(IOptions<GameStorageOptions> gameStorageOptions)
    {
        jsonSerializer = gameStorageOptions.Value.CreateJsonSerializer();
    }

    public async Task SaveAsync(Guid id, object? data)
    {
        await Task.Yield();
        var jsonData = data == null ? JValue.CreateNull() : JToken.FromObject(data, jsonSerializer);
        GameStorage.data.AddOrUpdate(id, jsonData, (_, _) => jsonData);
    }

    public async Task<Status<AsyncProcessed<T>>> UpdateAsync<T>(Guid id, Func<AsyncProcessed<T>, AsyncProcessed<T>> mutator)
    {
        await Task.Yield();
        if (!data.TryGetValue(id, out var original))
            return new Status<AsyncProcessed<T>>.Failure();

        var newValue = mutator(original.ToObject<AsyncProcessed<T>>(jsonSerializer)!);
        return GameStorage.data.TryUpdate(id, data == null ? JValue.CreateNull() : JToken.FromObject(newValue, jsonSerializer), original)
            ? new Status<AsyncProcessed<T>>.Success(newValue)
            : new Status<AsyncProcessed<T>>.Failure();
    }

    public Task SaveAsync<T>(Guid id, T data)
    {
        return SaveAsync(id, (object?)data);
    }

    public async Task<Status<T>> LoadAsync<T>(Guid id)
    {
        await Task.Yield();
        return data.ContainsKey(id)
            ? new Status<T>.Success(data[id].ToObject<T>(jsonSerializer)!)
            : new Status<T>.Failure();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        await Task.Yield();
        return data.TryRemove(id, out JToken _);
    }
}
