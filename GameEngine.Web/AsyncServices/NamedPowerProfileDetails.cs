using Azure;
using Azure.Data.Tables;
using GameEngine.Generator;
using GameEngine.Generator.Text;
using GameEngine.Web.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace GameEngine.Web.AsyncServices;

public record PowerDetails(Guid ParentClassId, Guid PowerId, FlavorText Flavor, ClassPowerProfile Profile) : Storage.IStorable<PowerDetails.PowerDetailsTableEntity, Storage.TableKey>
{
    public Storage.TableKey GetKey() => new Storage.TableKey(ParentClassId.ToString(), PowerId.ToString());

    public PowerDetailsTableEntity ToStorableEntity(Storage.TableKey id) =>
        new PowerDetailsTableEntity
        {
            PartitionKey = id.PartitionKey,
            RowKey = id.RowKey,
            FlavorJson = GameSerialization.ToJson(Flavor),
            ProfileJson = GameSerialization.ToJson(Profile),
        };

    public static PowerDetails FromTableEntity(PowerDetailsTableEntity entity) =>
        new PowerDetails(Guid.Parse(entity.PartitionKey), Guid.Parse(entity.RowKey), GameSerialization.FromJson<FlavorText>(entity.FlavorJson), GameSerialization.FromJson<ClassPowerProfile>(entity.ProfileJson));

    public class PowerDetailsTableEntity : ITableEntity
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        public string FlavorJson { get; set; }
        public string ProfileJson { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }

    public TableKey ToTableKey()
    {
        return ToTableKey(ParentClassId, PowerId);
    }

    public static TableKey ToTableKey(Guid classId, Guid powerId)
    {
        return new(classId.ToString(), powerId.ToString());
    }
}

public static class GameSerialization
{
    private static JsonSerializer? jsonSerializer;

    public static JsonSerializer JsonSerializer { set { jsonSerializer = value; } }

    public static string ToJson<T>(T data) => JToken.FromObject(data!, jsonSerializer!).ToString();
    public static T FromJson<T>(string json)
    {
        using var stringReader = new System.IO.StringReader(json);
        using var jsonTextReader = new JsonTextReader(stringReader);

        return jsonSerializer!.Deserialize<T>(jsonTextReader)!;
    }
}