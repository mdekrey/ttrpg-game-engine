
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;

namespace GameEngine.Web.Storage;
public class GameStorage
{
    private static readonly ConcurrentDictionary<Guid, JToken> data = new ();
    private readonly JsonSerializer jsonSerializer;

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

    public Task SaveAsync<T>(Guid id, T data)
    {
        return SaveAsync(id, (object?)data);
    }

    public async Task<T> LoadAsync<T>(Guid id)
    {
        await Task.Yield();
        return (data.ContainsKey(id) ? data[id].ToObject<T>(jsonSerializer)! : throw new KeyNotFoundException());
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        await Task.Yield();
        return data.TryRemove(id, out JToken _);
    }
}
