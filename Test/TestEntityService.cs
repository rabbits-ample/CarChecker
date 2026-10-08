using System.Net;
using System.Net.Http.Json;
using System.Text;
using Moq;
using Moq.Protected;
using Server;
using Server.Services;

namespace Test;

public class TestEntityService
{
    private object permitRestrictionPayload = new
    {
        Rsp = new
        {
            Status = "Ok",
            Result = new
            {
                ScheduledPermits = new[]
                {
                    new
                    {
                        Days = "Weekdays",
                        StartHour = "16:50:00",
                        EndHour = "23:59:00",
                        Permits = new
                        {
                            IsAllPermits = false, IsEveryone = false, IsNoPermits = false, Guids = new[] { "permitRestrictionGUID" }
                        }
                    }
                }
            }
        }
    };
    private object parkingZonePayload = new
    {
        Rsp = new
        {
            Status = "Ok",
            Result = new
            {
                ParkingZoneRules = new []{"parkingZoneRulesGUID"}
            }
        }
    };

    [Fact]
    public async void EntityService_Maps_To_ParkingZone()
    {
        // Arrange

        var parkingZoneClient = createMockHttpClient(HttpStatusCode.OK, "parkingZoneGUID", parkingZonePayload);
        var parkingZoneSevice = new EntityService<ParkingZone>(parkingZoneClient);
        
        // Act
        ParkingZone parkingZone = await parkingZoneSevice.GetEntityAsync("parkingZoneGUID");
        
        // Assert
        
        Assert.NotNull(parkingZone);
        Assert.Equal("parkingZoneRulesGUID", parkingZone.ParkingZoneRules[0]);

    }

    [Fact]
    public async void EntityService_Maps_To_PermitRestriction()
    {
        // Arrange

        var permitRestrictionClient = createMockHttpClient(HttpStatusCode.OK, "permitRestrictionGUID", permitRestrictionPayload);
        var permitRestrictionService = new EntityService<PermitRestriction>(permitRestrictionClient);
        
        // Act
        PermitRestriction permitRestriction = await permitRestrictionService.GetEntityAsync("permitRestrictionGUID");
    
        // Assert
        Assert.NotEmpty(permitRestriction.ScheduledPermits);
        Assert.Equal("Weekdays",permitRestriction.ScheduledPermits[0].Days);
        Assert.Equal("16:50:00",permitRestriction.ScheduledPermits[0].StartHour);
        Assert.Equal("Weekdays",permitRestriction.ScheduledPermits[0].Days);
        Assert.Equal("permitRestrictionGUID",permitRestriction.ScheduledPermits[0].Permits.Guids[0]);
    }
    
    public HttpClient createMockHttpClient(HttpStatusCode statusCode,  string path, object? content = null) // this could be made into a reusble helper/util....
    {
        // Set up a mock HttpMessageHandler to control the HttpClient's behavior.
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.Method == HttpMethod.Get &&
                    r.RequestUri.AbsolutePath.EndsWith($"entity/{path}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage // Returns a successful HTTP response
            {
                StatusCode = statusCode,
                Content = JsonContent.Create(content) // Create is used on an object! not a string.
            });

        // Create an HttpClient instance using the mocked handler.
        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        httpClient.BaseAddress = new Uri("https://fake.token.endpoint");
        return httpClient;
    }
}