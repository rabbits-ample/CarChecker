namespace CarChecker_Real;

/// <summary>
/// Class for retrieving token service
/// </summary>
public class TokenService: ITokenService
{
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;
    public TokenService(IConfiguration config, IHttpClientFactory httpClientFactory)
    {
        _config = config;
        _httpClient = httpClientFactory.CreateClient("Paylock");
    }
    public bool IsTokenValid(Token token)
    {
        if (!string.IsNullOrEmpty(token.AccessToken))
        {
            Console.WriteLine($"Current Time {DateTime.Now.ToString("hh:mm:ss tt")}");
            var buffer = 60; // A token that expires in 1 second should not be valid
            var expiresAt = token.RetrievedAt.AddSeconds(token.ExpiresIn - buffer);
            if (DateTime.Now < expiresAt)
            {
                String expiresString = expiresAt.ToString("hh:mm:ss tt");
                Console.WriteLine($"Token is valid until {expiresString} ");
                return true;
            }
        }
        Console.WriteLine("Token is not valid, refreshing token");
        return false;
    }

    public async Task<Token> GetTokenAsync(Token token)
    {
        // I need to learn how to get the secrets, and then submit those to the http url with those credentials
        // and to return the object as a token. 
        if (!IsTokenValid(token))
        {
            var clientId = _config["Credentials:ClientId"];
            if (string.IsNullOrWhiteSpace(clientId))
                throw new InvalidOperationException("Configuration error: 'SyncSettings:ClientId' is missing or empty.");
            
            var clientSecret = _config["Credentials:ClientSecret"];
            if (string.IsNullOrWhiteSpace(clientSecret))
                throw new InvalidOperationException("Configuration error: 'SyncSettings:ClientSecret' is missing or empty.");
            
            token.TokenType = "Valid";
            token.AccessToken = "FAKE_ACCESS_TOKEN";
            token.ExpiresIn = 3600;
            token.RetrievedAt = DateTime.Now;

        }
       
        return token;
        /*var clientId = _configuration["SyncSettings:ClientId"];
        if (string.IsNullOrWhiteSpace(clientId))
            throw new InvalidOperationException("Configuration error: 'SyncSettings:ClientId' is missing or empty.");

        var clientSecret = _configuration["SyncSettings:ClientSecret"];
        if (string.IsNullOrWhiteSpace(clientSecret))
            throw new InvalidOperationException("Configuration error: 'SyncSettings:ClientSecret' is missing or empty.");

        var tokenRequest = new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
            ["scope"] = ""
        };

        var response = await Utils.Retry.Execute(() =>
            _httpClient.PostAsync(_configuration["SyncSettings:TokenUri"], new FormUrlEncodedContent(tokenRequest)));

        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();

        if (tokenResponse == null)
        {
            Console.WriteLine("Token response null. Failed to get token.");
            return null;
        }

        return tokenResponse.AccessToken;*/
    }

}