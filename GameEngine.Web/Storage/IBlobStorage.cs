using System;
using System.Threading.Tasks;

namespace GameEngine.Web.Storage;

public interface IBlobStorage<T>
{
    Task<bool> DeleteAsync(Guid id);
    Task<StorageStatus<T>> LoadAsync(Guid id);
    Task SaveAsync(Guid id, T data);
}
