namespace Server.Services;

public interface IHandleHitService
{
    public Task ProcessReadAsync(string plate);
}