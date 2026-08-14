namespace CarChecker_Real;
/// <summary>
/// Class for platelookup service
/// </summary>
public class PlateLookupService : IPlateLookupService
{

    private readonly HttpClient _httpClient;
    public PlateLookupService(ITokenService tokenService, IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("Paylock");
    }

    public async Task<Car> GetCarInfoAsync(string licensePlateNumber,string accessToken)
    {
        var response = await _httpClient.GetAsync($"path/{accessToken}/{licensePlateNumber}");
        response.EnsureSuccessStatusCode();
        // Maybe potentially do a fallback -> if for some reason either an expired token gets through, it crashes, or whatever. Do we retry? 
        return await response.Content.ReadFromJsonAsync<Car>();
    }
}