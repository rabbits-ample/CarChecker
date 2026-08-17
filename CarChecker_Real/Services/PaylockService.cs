using System.Net.Http.Headers;

namespace CarChecker_Real.Services;
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

    public async Task<Car> GetCarInfoAsync(string licensePlateNumber)
    {
        var clientId = _config["Paylock:ClientId"];
        var clientSecret = _config["Paylock:ClientSecret"];
        Token token = await _tokenService.GetTokenAsync(clientId, clientSecret);
        
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        //var response = await _httpClient.GetAsync($"path/{licensePlateNumber}");
        //response.EnsureSuccessStatusCode();
        // Maybe potentially do a fallback -> if for some reason either an expired token gets through, it crashes, or whatever. Do we retry? 
        //return await response.Content.ReadFromJsonAsync<Car>();
        return new Car { Active = false, Registered = true };
    }
}