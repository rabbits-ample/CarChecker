namespace Server;
/// <summary>
/// Interface for platelookup service
/// </summary>
public interface IPaylockService
{
    public Task<Car> GetCarInfoAsync(string plate);
}