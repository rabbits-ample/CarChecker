using System.Net;
using System.Net.Http.Headers;

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
        Token token = await _tokenService.GetTokenAsync(clientId, clientSecret);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken); // I know that this sets the default. There is a way to attach to request instead of the other way around. 
        var response = await _httpClient.PostAsJsonAsync($"path/{phoneNumber}", warningText); // I don't know what this is supposed to look like
        return response.StatusCode;
    }
}