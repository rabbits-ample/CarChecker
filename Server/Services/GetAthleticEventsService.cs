namespace Server.Services;

public class GetAthleticEventsService
{
    HttpClient _httpClient;

    public GetAthleticEventsService(HttpClient httpClient)
    {
        //                                                              dont forget to speciify .json
        _httpClient = httpClient; // https://calendar.byu.edu/api/Events.json?categories=10&event%5Bmin%5D%5Bdate%5D=2026-10-06&event%5Bmax%5D%5Bdate%5D=2026-12-28
    }
    
    public async Task<List<Event>> GetEvents()
    {
        List<String> keywords = new() { "Athletic","Football","Basketball"};
        
        var response = await _httpClient.GetAsync("events");
        var events = await response.Content.ReadFromJsonAsync<List<Event>>();
        
       // var events = await _httpClient.GetFromJsonAsync<List<Event>>("events");
        var athleticEvents = events.Where(e => keywords.Any(k => e.Title.Contains(k)));
        return athleticEvents.ToList();
    } 
}