
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace GameEngine.Web.Storage;

public class InMemoryTableStorage<T> : ITableStorage<T>
{
    private static readonly ConcurrentDictionary<TableKey, JToken> data = new();
    private readonly JsonSerializer jsonSerializer;

    public InMemoryTableStorage(IOptions<GameStorageOptions> gameStorageOptions)
    {
        jsonSerializer = gameStorageOptions.Value.CreateJsonSerializer();
    }

    public async Task SaveAsync(TableKey id, T target)
    {
        await Task.Yield();
        var jsonData = target == null ? JValue.CreateNull() : JToken.FromObject(target, jsonSerializer);
        data.AddOrUpdate(id, jsonData, (_, _) => jsonData);
    }

    public async Task<StorageStatus<T>> LoadAsync(TableKey id)
    {
        await Task.Yield();
        return data.ContainsKey(id)
            ? new StorageStatus<T>.Success(data[id].ToObject<T>(jsonSerializer)!)
            : new StorageStatus<T>.Failure();
    }

    public async Task<bool> DeleteAsync(TableKey id)
    {
        await Task.Yield();
        return data.TryRemove(id, out JToken _);
    }

    public IAsyncEnumerable<T> Query(Expression<Func<TableKey, T, bool>> filter)
    {
        // This compilation should be cached for production loads
        var compiled = filter.Compile();
        return data.Select(kvp => (kvp.Key, Value: kvp.Value.ToObject<T>(jsonSerializer)!)).Where(kvp => compiled(kvp.Key, kvp.Value)).Select(kvp => kvp.Value).ToAsyncEnumerable();
    }
}
