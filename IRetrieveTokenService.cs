namespace CarChecker_Real;
/// <summary>
/// Interface for retrieving token service
/// </summary>
public interface IRetrieveTokenService
{
    public bool IsTokenValid();
    public Token RefreshToken();
}