using System.Net;
using Server.Services;

namespace Test.Helpers;

public class TestGetAthleticEventsService
{
    [Fact]
    public async void TestGetAthleticEvents()
    {
        var eventPayload = new[]
        {
                new
                {
                    Title = "Athletic Event",
                    StartDateTime = "Athletic Event",
                },
                new
                {
                    Title = "Football wins",
                    StartDateTime = "Athletic Event",
                }
        };
        CreateMockClient client = new(HttpStatusCode.OK, "events", eventPayload);
        GetAthleticEventsService eventService = new(client.HttpClient);
        var athleticEvents = await eventService.GetEvents();
    }
}