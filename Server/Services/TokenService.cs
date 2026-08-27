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
        // I need to learn how to get the secrets, and then submit those to the http url with those credentials
        // and to return the object as a token. 
        tokenShelf.Tokens.TryGetValue(credentials, out Token token);
        if (token == null || !IsTokenValid(token))
        {
            /*var request = new HttpRequestMessage(HttpMethod.Get, path);
            request.Content = credentials;
            var response = await httpClient.SendAsync(request);*/
  
            try
            {
                var response = await Utils.Retry.Execute(() => httpClient.PostAsync(path, credentials));
                Token newToken = await response.Content.ReadFromJsonAsync<Token>();
            
                if (newToken == null)
                {
                    Console.WriteLine("Token response null. Failed to get token.");
                    return null;
                    // maybe instead of this, if token is null ( depending on the response code or whatever) you can retry 3 times with a small bugger
                }

                tokenShelf.Tokens[credentials] = newToken;
                return newToken;
            }
            catch (HttpRequestException e)
            {
                // throwing an error here gets ignored, and I don't know why
                Console.WriteLine($"Could not retrieve token from path {httpClient.BaseAddress}{path}: {e.Message}");
            }
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
            _httpClient.PostAsync(_configuration["SyncSettings:TokenUri"], new FormUrlEncodedContent(tokenRequest)));*/
    }

}