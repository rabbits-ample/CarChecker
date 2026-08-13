namespace CarChecker_Real;

/// <summary>
/// Class for retrieving token service
/// </summary>
public class RetrieveTokenService: IRetrieveTokenService
{
    private readonly IConfiguration _config;
    public RetrieveTokenService(IConfiguration config)
    {
        _config = config;
    }
    public bool IsTokenValid()
    {
        return false;
    }

    public Token RefreshToken()
    {
        // I need to learn how to get the secrets, and then submit those to the http url with those credentials
        // and to return the object as a token. 
        var username = _config["Credentials:Username"];
        var password = _config["Credentials:Password"];
        return null;
    }

}