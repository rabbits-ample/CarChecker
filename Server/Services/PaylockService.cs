using System.Net.Http.Headers;

namespace Server.Services;
/// <summary>
/// Class for PaylockService service
/// </summary>
public class PaylockService : IPaylockService
{
    private readonly HttpClient _httpClient;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _config;
    private readonly TokenShelf _tokenShelf;
    public PaylockService(TokenShelf tokenShelf, IHttpClientFactory httpClientFactory,IConfiguration config)
    {
        _httpClient = httpClientFactory.CreateClient("Paylock");
        var _tokenShelf = tokenShelf;
        _tokenService = new TokenService(_httpClient,_tokenShelf);
        _config = config;
    }

    public async Task<Car> GetCarInfoAsync(string plate)
    {
        await Authenticate();
        
            var response = await _httpClient.GetAsync($"path/{plate}");
            response.EnsureSuccessStatusCode();
           
            var car = await response.Content.ReadFromJsonAsync<Car>();
            
            if (car == null)
            {
                Console.WriteLine($"Car with license plate # '{plate}' not found.");
                return null;
            }

            return car;
        
    }
    public async Task Authenticate()
    {
        var clientId = _config["Paylock:ClientId"];
        var clientSecret = _config["Paylock:ClientSecret"];
        
        if (string.IsNullOrWhiteSpace(clientId))
            throw new InvalidOperationException("Configuration error: 'ClientId' is missing or empty.");
        if (string.IsNullOrWhiteSpace(clientSecret))
            throw new InvalidOperationException("Configuration error: 'ClientSecret' is missing or empty.");
        
        var credentials = new StringContent($"{{\r\n  \"email\": \"{clientId}\",\r\n  \"password\": \"{clientSecret}\"\r\n}}", null, "text/plain");
        
        Token token = await _tokenService.GetTokenAsync("path",credentials);
        // token could be null here? do a check
        
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

    }
}