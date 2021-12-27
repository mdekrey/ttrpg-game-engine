namespace GameEngine.Web.Storage;

public interface IStorable<out T, in TKey>
{
    T ToStorableEntity(TKey id);
}

