namespace CarChecker_Real;
/// <summary>
/// Interface for platelookup service
/// </summary>
public interface IPaylockService
{
    public Task<Car> GetCarInfoAsync(string licensePlateNumber);
}