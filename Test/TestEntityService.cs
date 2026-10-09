using System.Net;
using System.Net.Http.Json;
using System.Text;
using Moq;
using Moq.Protected;
using Server;
using Server.Services;
using Test.Helpers;

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
        CreateMockClient parkingZoneClient = new(HttpStatusCode.OK, "parkingZoneGUID", parkingZonePayload);

        //var parkingZoneClient = createMockHttpClient();
        var parkingZoneSevice = new EntityService<ParkingZone>(parkingZoneClient.HttpClient);
        
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

        CreateMockClient permitRestrictionClient = new(HttpStatusCode.OK, "permitRestrictionGUID", permitRestrictionPayload);
        var permitRestrictionService = new EntityService<PermitRestriction>(permitRestrictionClient.HttpClient);
        
        // Act
        PermitRestriction permitRestriction = await permitRestrictionService.GetEntityAsync("permitRestrictionGUID");
    
        // Assert
        Assert.NotEmpty(permitRestriction.ScheduledPermits);
        Assert.Equal("Weekdays",permitRestriction.ScheduledPermits[0].Days);
        Assert.Equal("16:50:00",permitRestriction.ScheduledPermits[0].StartHour);
        Assert.Equal("Weekdays",permitRestriction.ScheduledPermits[0].Days);
        Assert.Equal("permitRestrictionGUID",permitRestriction.ScheduledPermits[0].Permits.Guids[0]);
    }
    
}