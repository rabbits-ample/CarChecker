namespace CarChecker_Real;
/// <summary>
/// Interface for platelookup service
/// </summary>
public interface IPlateLookupService
{
    public  Task<Car> GetCarInfoAsync(string licensePlateNumber, string accessToken);
}