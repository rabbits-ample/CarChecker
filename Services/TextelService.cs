namespace CarChecker_Real;

public class TextelService: ITextelService
{
    private readonly HttpClient _httpClient;
    public TextelService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("Textel");
    }

    public async Task sendTextAsync(string warningText,string phoneNumber)
    {
        // I also just now realized, that I am unsure if Textel also will require a Token or whatever
        _httpClient.PostAsJsonAsync($"path/{phoneNumber}", warningText); // I don't know what this is supposed to look like
    }
}