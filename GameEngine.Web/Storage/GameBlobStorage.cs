
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GameEngine.Web.Storage;
public class GameBlobStorage : IGameStorage
{
    private readonly JsonSerializer jsonSerializer;
    private readonly BlobContainerClient containerClient;

    public GameBlobStorage(IOptions<GameStorageOptions> gameStorageOptions, BlobContainerClient containerClient)
    {
        jsonSerializer = gameStorageOptions.Value.CreateJsonSerializer();
        this.containerClient = containerClient;
    }

    public async Task SaveAsync(Guid id, object? data)
    {
        var jsonData = data == null ? JValue.CreateNull() : JToken.FromObject(data, jsonSerializer);
        var blobClient = containerClient.GetBlobClient(id.ToString());
        using var memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonData.ToString()));
        await blobClient.UploadAsync(memoryStream, overwrite: true);
    }

    public async Task<StorageStatus<AsyncProcessed<T>>> UpdateAsync<T>(Guid id, Func<AsyncProcessed<T>, AsyncProcessed<T>> mutator)
    {
        var originalAsyncValue = await LoadAsync<AsyncProcessed<T>>(id);
        if (originalAsyncValue is not StorageStatus<AsyncProcessed<T>>.Success { Value: AsyncProcessed<T> originalValue })
            return new StorageStatus<AsyncProcessed<T>>.Failure();

        var newValue = mutator(originalValue);
        await SaveAsync(id, newValue);
        return new StorageStatus<AsyncProcessed<T>>.Success(newValue);
    }

    public async Task<StorageStatus<T>> LoadAsync<T>(Guid id)
    {
        var blobClient = containerClient.GetBlobClient(id.ToString());

        if (!await blobClient.ExistsAsync())
            return new StorageStatus<T>.Failure();

        var response = await blobClient.DownloadContentAsync();
        using var stream = response.Value.Content.ToStream();
        using var streamReader = new StreamReader(stream, System.Text.Encoding.UTF8);
        using var jsonTextReader = new JsonTextReader(streamReader);

        return new StorageStatus<T>.Success(jsonSerializer.Deserialize<T>(jsonTextReader)!);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var blobClient = containerClient.GetBlobClient(id.ToString());
        var response = await blobClient.DeleteIfExistsAsync();
        return response.Value;
    }
}
