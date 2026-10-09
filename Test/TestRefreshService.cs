using Moq;
using Server;
using Server.Interfaces;
using Server.Services;

namespace Test;

public class TestRefreshService
{
    private readonly string parkingZoneRulesId = "fakeParkingZoneRulesId";
    private readonly string permitRestrictionId = "fakePermitRestrictionId";
    
    
    [Fact]
    public async Task Test()
    {
        // Arrange
        var parkingZoneService = new Mock<IEntityService<ParkingZone>>();
        parkingZoneService.Setup(x => x.GetEntityAsync(It.IsAny<String>())).ReturnsAsync(new ParkingZone()
        {
            ParkingZoneRules = new List<string>() {parkingZoneRulesId}
        });
        
        var parkingZoneRuleService = new Mock<IEntityService<ParkingZoneRule>>();
        parkingZoneRuleService.Setup(x => x.GetEntityAsync(parkingZoneRulesId)).ReturnsAsync(new ParkingZoneRule()
        {
            Permit = permitRestrictionId 
        });

        
        var permitRestrictionService = new Mock<IEntityService<PermitRestriction>>();
        permitRestrictionService.Setup(x => x.GetEntityAsync(permitRestrictionId)).ReturnsAsync(new PermitRestriction()
        {
            ScheduledPermits = new List<ScheduledPermit>() { new (){ StartHour = "16:40:00", EndHour = "23:59:00" , Permits = new(){ IsEveryone = true}} }
        });
        
        RefreshService refreshService = new RefreshService(parkingZoneService.Object,parkingZoneRuleService.Object,permitRestrictionService.Object);
        await refreshService.ExecuteAsync();
        
    }
}