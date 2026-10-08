namespace Server.Interfaces;

public interface IEntityService<T>
{
    public Task<T> GetEntityAsync(string guid);
    
}