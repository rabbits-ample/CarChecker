namespace Server;

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

    public async Task<Token> GetTokenAsync(string path, StringContent credentials)
    {
        tokenShelf.Tokens.TryGetValue(credentials, out Token token);
        if (token == null || !IsTokenValid(token))
        {
            try
            {
                var response = await Utils.Retry.Execute(() => httpClient.PostAsync(path, credentials));
                Token newToken = await response.Content.ReadFromJsonAsync<Token>();

                tokenShelf.Tokens[credentials] = newToken;
                return newToken;
            }
            catch (AggregateException ex)
            {
                Console.WriteLine($"Could not retrieve token from path {httpClient.BaseAddress}{path}: {ex.Message}");
            }
        }
        return token;
    }

}