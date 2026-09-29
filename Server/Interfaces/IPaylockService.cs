namespace Server;
/// <summary>
/// Interface for platelookup service
/// </summary>
public interface IPaylockService
{
    public Task<HttpResponseMessage> GetCarInfoAsync(string plate);
}