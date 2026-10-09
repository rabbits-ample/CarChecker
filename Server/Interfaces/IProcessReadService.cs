namespace Server.Services;

public interface IProcessReadService
{
    public Task ProcessReadAsync(string plate);
}