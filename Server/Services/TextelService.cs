using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Server;

public class TextelService: ITextelService
{
    private readonly HttpClient _httpClient;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _config;
    private readonly TokenShelf _tokenShelf;
    public TextelService(TokenShelf tokenShelf, IHttpClientFactory httpClientFactory,IConfiguration config)
    {
        _httpClient = httpClientFactory.CreateClient("Paylock");
        _tokenShelf = tokenShelf;
        _tokenService = new TokenService(_httpClient,_tokenShelf);
        _config = config;
    }

    public async Task<HttpStatusCode> sendTextAsync(string warningText,string phoneNumber)
    {
        var clientId = _config["Textel:ClientId"];
        var clientSecret = _config["Textel:ClientSecret"];
        if (string.IsNullOrWhiteSpace(clientId))
            throw new InvalidOperationException("Configuration error: 'ClientId' is missing or empty.");
        if (string.IsNullOrWhiteSpace(clientSecret))
            throw new InvalidOperationException("Configuration error: 'ClientSecret' is missing or empty.");
        
        var content = new StringContent($"{{\r\n  \"email\": \"{clientId}\",\r\n  \"password\": \"{clientSecret}\"\r\n}}", null, "text/plain");
        
        Token token = await _tokenService.GetTokenAsync("auth/authenticate",content);
        // token could be null here
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken); // I know that this sets the default. There is a way to attach to request instead of the other way around. 
        var response = await _httpClient.PostAsJsonAsync($"path/{phoneNumber}", warningText); // I don't know what this is supposed to look like
        return response.StatusCode;
    }
}