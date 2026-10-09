using Server.Interfaces;

namespace Server.Services;

public class RefreshService //: BackgroundService // Right here quartz would go?
{
    private readonly IEntityService<ParkingZone> _parkingZoneService; // Interface allows it to be mocked and tested
    private readonly IEntityService<ParkingZoneRule> _parkingZoneRuleService;
    private readonly IEntityService<PermitRestriction> _permitRestrictionService;
    
    private readonly LotScheduleManager _lotScheduleManager = new();
    private List<String> _parkingZoneGUIDS = new() {"real","real","real"}; // This may be defined and accessed only here. It also could be defined in the database???

    public RefreshService(IEntityService<ParkingZone> parkingZoneService,
        IEntityService<ParkingZoneRule> parkingZoneRuleService, IEntityService<PermitRestriction> permitRestrictionService)
    {
        //_httpClient = httpClientFactory.CreateClient("Genetec"); // because this doesn't happen here, remember to initialize these with Genetec client still in it's parent
        _parkingZoneService = parkingZoneService;
        _parkingZoneRuleService = parkingZoneRuleService;
        _permitRestrictionService = permitRestrictionService;
    }

    //protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    public async Task ExecuteAsync()
    {
        ParkingMapDatabase parkingMapDatabase = new();
        var refreshedZones = await GetUpdatedParkingZones();
        
        // we should expect this to not fail at this point -> if there is an error beforehand, then it should jump beyond this.
        SaveSwapDatabase(refreshedZones);
        
    }

    private async Task<List<ParkingZone>> GetUpdatedParkingZones()
    {
        List<ParkingZone> refreshedZones = new();
        foreach (string guid in _parkingZoneGUIDS)
        {
            // we will assume that error handing will occur internally or get escalated out.
            ParkingZone parkingZone = await _parkingZoneService.GetEntityAsync(guid);
            parkingZone.Id = guid;
            
            ParkingZoneRule rule = await _parkingZoneRuleService.GetEntityAsync(parkingZone.ParkingZoneRules[0]); // it seems like the Genetec convention is only having 1 rule. Although it is stored in a list
            PermitRestriction permitRestriction = await _permitRestrictionService.GetEntityAsync(rule.Permit);

            var everyonePermit =  permitRestriction.ScheduledPermits.FirstOrDefault(sp => sp.Permits.IsEveryone);
            if (everyonePermit != null)
            {
                var startEnforced = _lotScheduleManager.ConvertStrToTimespan(everyonePermit.StartHour);
                var endEnforced = _lotScheduleManager.ConvertStrToTimespan(everyonePermit.EndHour);

                parkingZone.StartEnforced = startEnforced;
                parkingZone.EndEnforced = endEnforced;

            }
            refreshedZones.Add(parkingZone);
        }

        return refreshedZones;
    }

    private void SaveSwapDatabase(List<ParkingZone> parkingZones)
    {
        
    }

}