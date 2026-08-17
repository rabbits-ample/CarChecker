namespace CarChecker_Real;

/// <summary>
/// Class for retrieving token service
/// </summary>
public class TokenService(HttpClient httpClient ,TokenShelf tokenShelf ): ITokenService
{
    public bool IsTokenValid(Token token)
    {
        if (!string.IsNullOrEmpty(token.AccessToken))
        {
            Console.WriteLine($"Current Time {DateTime.Now.ToString("hh:mm:ss tt")}");
            var buffer = 60; // A token that expires in 1-60 seconds should not be valid
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

    public async Task<Token> GetTokenAsync(string clientId, string clientSecret )
    {
        // I need to learn how to get the secrets, and then submit those to the http url with those credentials
        // and to return the object as a token. 
        tokenShelf.Tokens.TryGetValue($"{clientId},{clientSecret}", out Token token);
        if (token == null || !IsTokenValid(token))
        {
            if (string.IsNullOrWhiteSpace(clientId))
                throw new InvalidOperationException("Configuration error: 'ClientId' is missing or empty.");
            if (string.IsNullOrWhiteSpace(clientSecret))
                throw new InvalidOperationException("Configuration error: 'ClientSecret' is missing or empty.");
           
            var response = await httpClient.GetAsync($"{clientId}");
            response.EnsureSuccessStatusCode();
            Token newToken = await response.Content.ReadFromJsonAsync<Token>();
            tokenShelf.Tokens[$"{clientId},{clientSecret}"] = newToken;
            return newToken;
        }
        return token;
        /*
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