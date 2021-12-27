
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace GameEngine.Web.Storage;
public class InMemoryBlobStorage<T> : IBlobStorage<T>
{
    private static readonly ConcurrentDictionary<Guid, JToken> data = new ();
    private readonly JsonSerializer jsonSerializer;

    public InMemoryBlobStorage(IOptions<GameStorageOptions> gameStorageOptions)
    {
        jsonSerializer = gameStorageOptions.Value.CreateJsonSerializer();
    }

    public async Task SaveAsync(Guid id, T target)
    {
        await Task.Yield();
        var jsonData = target == null ? JValue.CreateNull() : JToken.FromObject(target, jsonSerializer);
        data.AddOrUpdate(id, jsonData, (_, _) => jsonData);
    }

    public async Task<StorageStatus<T>> LoadAsync(Guid id)
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
