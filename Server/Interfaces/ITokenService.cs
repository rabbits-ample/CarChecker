namespace Server;
/// <summary>
/// Interface for retrieving token service
/// </summary>
public interface ITokenService
{
    public bool IsTokenValid(Token token);
    public Task<Token> GetTokenAsync( string path, StringContent content);
}