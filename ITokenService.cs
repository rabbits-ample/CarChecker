namespace CarChecker_Real;
/// <summary>
/// Interface for retrieving token service
/// </summary>
public interface ITokenService
{
    public bool IsTokenValid(Token token);
    public Task<Token> GetTokenAsync(Token token);
}