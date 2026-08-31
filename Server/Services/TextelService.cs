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

    public async Task<HttpResponseMessage> sendTextAsync(string warningText,string phoneNumber)
    {
        await Authenticate();
        var json = new
        {
            messageId =  "",
            to =  phoneNumber,
            from = "+8012839393",
            body =  warningText,
            attachmentUrl =  ""
        };
        var content = JsonSerializer.Serialize(json);
        var response = await _httpClient.PostAsJsonAsync($"message/send", content); 
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine("Failed to send warning text");
        }
        else
        {
            Console.WriteLine("Warning text was sent");
        }
        return response;
    }
    public async Task Authenticate()
    {
        var clientId = _config["Textel:ClientId"];
        var clientSecret = _config["Textel:ClientSecret"];
        if (string.IsNullOrWhiteSpace(clientId))
            throw new InvalidOperationException("Configuration error: 'ClientId' is missing or empty.");
        if (string.IsNullOrWhiteSpace(clientSecret))
            throw new InvalidOperationException("Configuration error: 'ClientSecret' is missing or empty.");
        
        var authentication = new StringContent($"{{\r\n  \"email\": \"{clientId}\",\r\n  \"password\": \"{clientSecret}\"\r\n}}", null, "text/plain");
        // textel has basic authentication -> which doesn't require a token. If Tyler wants this, then cool
        Token token = await _tokenService.GetTokenAsync("auth/authenticate",authentication);
        // token could be null here
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken); // I know that this sets the default. There is a way to attach to request instead of the other way around. 

    }
}