namespace Server.Services;

public class RefreshService : BackgroundService // Right here quartz would go?
{
    private EntityService<ParkingZone> _parkingZoneService;
    private EntityService<ParkingZoneRule> _parkingZoneRuleService;
    private EntityService<PermitRestriction> _permitRestrictionService;
    
    private HttpClient _httpClient;
    
    public LotScheduleManager lotScheduleManager = new();
    private List<String> ParkingZoneGUIDS = new(); // This may be defined and accessed only here. It also could be defined in the database???

    public RefreshService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("Genetec");
        
        _parkingZoneService = new(_httpClient);
        _parkingZoneRuleService = new(_httpClient);
        _permitRestrictionService = new(_httpClient);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ParkingMapDatabase parkingMapDatabase = new();
        var refreshedZones = await GetUpdatedParkingZones();
        
        // we should expect this to not fail at this point -> if there is an error beforehand, then it should jump beyond this.
        SaveSwapDatabase(refreshedZones);
        
    }

    private async Task<List<ParkingZone>> GetUpdatedParkingZones()
    {
        List<ParkingZone> refreshedZones = new();
        foreach (string guid in ParkingZoneGUIDS)
        {
            // we will assume that error handing will occur internally or get escalated out.
            ParkingZone parkingZone = await _parkingZoneService.GetEntityAsync(guid);
            parkingZone.Id = guid;
            
            ParkingZoneRule rule = await _parkingZoneRuleService.GetEntityAsync(parkingZone.ParkingZoneRules[0]); // it seems like the Genetec convention is only having 1 rule. Although it is stored in a list
            PermitRestriction permitRestriction = await _permitRestrictionService.GetEntityAsync(rule.Permit);

            var everyonePermit =  permitRestriction.ScheduledPermits.FirstOrDefault(sp => sp.Permits.IsEveryone);
            if (everyonePermit != null)
            {
                var startEnforced = lotScheduleManager.ConvertStrToTimespan(everyonePermit.StartHour);
                var endEnforced = lotScheduleManager.ConvertStrToTimespan(everyonePermit.EndHour);

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