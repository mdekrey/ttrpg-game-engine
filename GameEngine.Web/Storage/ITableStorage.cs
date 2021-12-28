using Azure;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace GameEngine.Web.Storage;

public interface ITableStorage<T>
{
    Task<bool> DeleteAsync(TableKey id);
    Task<StorageStatus<T>> LoadAsync(TableKey id);
    Task SaveAsync(TableKey id, T data);
    IAsyncEnumerable<KeyValuePair<TableKey, T>> Query(Expression<Func<TableKey, T, bool>> filter);
}
