using Azure;
using Azure.Data.Tables;
using GameEngine.Generator;
using GameEngine.Web.Storage;
using System;
using System.Linq;

namespace GameEngine.Web.AsyncServices;

public enum ProgressState
{
    InProgress,
    Finished,
    Locked,
    Deleted,
}

public record ClassDetails(string Name, string Description, ClassProfile ClassProfile, ProgressState ProgressState) : Storage.IStorable<ClassDetails.ClassDetailsTableEntity, Storage.TableKey>
{
    public ClassDetailsTableEntity ToStorableEntity(Storage.TableKey id) =>
        new ClassDetailsTableEntity
        {
            PartitionKey = id.PartitionKey,
            RowKey = id.RowKey,
            Name = Name,
            Description = Description,
            ClassProfileJson = GameSerialization.ToJson(ClassProfile),
            ProgressState = ProgressState,
        };

    public static ClassDetails FromTableEntity(ClassDetailsTableEntity entity) =>
        new ClassDetails(entity.Name, entity.Description, GameSerialization.FromJson<ClassProfile>(entity.ClassProfileJson), entity.ProgressState);

    public class ClassDetailsTableEntity : ITableEntity
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ClassProfileJson { get; set; }
        public ProgressState ProgressState { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }

    public static TableKey ToTableKey(Guid classId)
    {
        var bytes = classId.ToByteArray();
        return new(Convert.ToBase64String(bytes[0..8]), Convert.ToBase64String(bytes[8..16]));
    }

    public static Guid IdFromTableKey(TableKey tableKey)
    {
        return new Guid(Convert.FromBase64String(tableKey.PartitionKey).Concat(Convert.FromBase64String(tableKey.RowKey)).ToArray());
    }
}
