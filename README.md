Car Checker Documentation

This is a cleaned-up compilation of my working notes on Car Checker: how reads flow through the app, how we pull and store schedules from Genetec, how the refresh service works, and how Hangfire and Docker fit in.

1. How a Read Gets Processed

When a car passes an LPR, the read is sent to our app. We can tell which LPR the read came from. We will have a hardcoded, manually created list that tells us which LPRs are associated with which lots. The LPR name is the key, and the value is a list of GUIDs that point to Parking Zone/Lot objects (which have a simplified schedule property).

The flow is:
Read → LPR name → look up LPR name → list of parking zone GUIDs → determine the combined schedule → decide what to do next

We still have to retrieve the schedules from Genetec. We are looking for the schedule of each specific parking lot/zone.

Weekends and holidays.
I realized that on weekends we can assume Car Checker should just be turned off, since enforcement shouldn't happen. The schedule would still technically work with a Saturday/Sunday day of the week, but it should never get that far, because Quartz will turn off the listen service on weekends and at the start of holidays, games, and so on.


2. How Genetec Stores Schedules

The Genetec format is:
Parking Zone → Parking Zone Rule → Permit Restriction

We can't just get permit restrictions by GUID, because we need to know which parking zone has which permit restrictions. So we need a list of GUIDs for either the parking zones or the parking zone rules. Since we are manually deciding which ones to implement, we will seed/hardcode this list into the program.

Why a list of zone or rule GUIDs? Right now, Genetec has every parking zone paired with a matching parking zone rule (1-to-1). If they keep doing that, retrieving the parking zone rule represents the parent parking zone, and we can skip retrieving the parking zone entity altogether. If they change it so a rule can have multiple zones (1-to-many), a rule no longer reflects one specific zone. Then we would have to retrieve the parking zone, then the parking zone rule, then the permit restriction.

The part of the response we care about is the ScheduledPermits section. It looks something like this:

"ScheduledPermits": [
                {
                    "Days": "Weekdays",
                    "EndHour": "23:00:00",
                    "Period": {
                        "FirstDay": 1,
                        "FirstMonth": 1,
                        "IsEmpty": false,
                        "LastDay": 31,
                        "LastMonth": 12
                    },
                    "Permits": {
                        "IsAllPermits": false,
                        "IsEveryone": true,
                        "IsNoPermits": false,
                        "Guids": [
                            "0362449c-e193-4747-9d66-e8b483069f1c"
                        ]
                    },
                    "StartHour": "17:00:00"
                },
                {
                    "Days": "Always",
                    "EndHour": "00:00:00",
                    "Period": {
                        "FirstDay": 1,
                        "FirstMonth": 1,
                        "IsEmpty": false,
                        "LastDay": 31,
                        "LastMonth": 12
                    },
                    "Permits": {
                        "IsAllPermits": false,
                        "IsEveryone": false,
                        "IsNoPermits": false,
                        "Guids": [
                            "6414474a-e5cd-4769-86eb-45ee5337335e"
                        ]
                    },
                    "StartHour": "00:00:00"
                }

We should also be able to get the permit entities through the API using their GUIDs, so we can query for a permit's name (for example, permit GUID 6414474a-e5cd-4769-86eb-45ee5337335e has the name "37Y"). I don't know yet how well that works out.

We only care about permit schedules where IsEveryone is true. That window dictates when everyone can be there, which means it tells us when the lot is not enforced on that day. So we really just need to collect all of the EVERYONE timespans and set them off to the side.
Schedules and midnight. None of the permit schedules go into the morning of the next day. They could be set up that way, though, and I'm not sure what that would look like on the receiving end (whether it would automatically export the actual day or not). Test this at some point: change the range a little and export it in test mode.

Storing the schedule: two options

1.     Take the inverse of the EVERYONE schedule range, convert it to a TimeSpan object, and set that as the weekday schedule property of a ParkingZone.
2.     Save all the objects in memory and interpret the schedule on the fly for each read.

I lean toward option 1. It saves time on every read, at the cost of making refreshes more expensive. A refresh would just save the ParkingZone object to the "database," where it can be retrieved quickly, and it would overwrite the object if it already exists.

I have been researching the TimeSpan object, and I like what I see. It is a perfect use for us.

Keeping object identity across refreshes

Right now, a GUID is created when you create an object, but a refresh retrieves and creates new ParkingZone instances. So how does the program know it's the same object? Since the ParkingZone GUIDs will already be hardcoded somewhere in our code, and we use them to retrieve from Genetec, we can set each object's GUID to the hardcoded GUID. That should work out nicely.


3. ParkingMapDatabase and LPR Lookup

We need a structure that stores date ranges, plus models to handle the data in memory. If we want to save to a real database, we can convert the models to database entities. We need to store Parking Zones, Parking Rules, and Permit Restrictions. For the zone, we need its rules; for the rules, we need the permit restrictions.
ParkingMapDatabase is a shared instance. I think it has to be a singleton, and the refresh service and the listen service both use that same instance.


public class ParkingMapDatabase
{
    private readonly Dictionary<string, List<string>> Lprs = new()
    {
        ["LprName"] = ["ParkingZone1"] // I'm thinking that this might just be a list to ZONE GUIDs?
    };
    
    public Dictionary<string, (TimeSpan,TimeSpan)> LprSchedules = new() { }; // This gets populated on refresh? key is Lpr name, value is (minstart, maxend)

    Dictionary<Type, List<BaseClass>> Tables = new();

    public T? GetById<T>(string id) where T:BaseClass 
    {
        return Tables[typeof(T)].FirstOrDefault(x => x.Id == id) as T;
    }

    public void Save<T>(T entity) where T : BaseClass
    {
        if(!Tables.ContainsKey(typeof(T))){
            Tables[typeof(T)] = [];
        }
        Tables[typeof(T)].Add(entity);
    }
}


The actual parking zone entity is then accessed through the ParkingMapDatabase class.

public class ParkingZone : BaseClass
{
    /// <summary>
    /// ZoneName 
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// GenetecGUID
    /// </summary>
    public string Guid {get; set;}
    
    /// <summary>
    ///  The genetec guid for a ParkingZoneRule object
    /// </summary>
    public List<String> ParkingZoneRules {get; set;}
    
    /// <summary>
    ///  The timespan representing the daily (weekday) enforced start time for the parkingZone 
    /// </summary>
    public TimeSpan StartEnforced {get; set;}
    
    /// <summary>
    ///  The timespan representing the daily (weekday) enforced end time for the parkingZone
    /// </summary>
    public TimeSpan EndEnforced {get; set;}
}


StartEnforced and EndEndforced are timespan objects that represent a neutral time of day. (Non specific to any particular day of the week). These would be accessed on receive read, to be able to determine if currentTime is in those TimeSpan. This feature is already configured with LotScheduleManager.cs


public class LotScheduleManager
{
    
    public bool IsEnforcedNow(TimeSpan start, TimeSpan end)
    {
        TimeSpan now = DateTime.Now.TimeOfDay;
        return IsEnforcedAt(start, end, now);
    }
    
    public bool IsEnforcedAt(TimeSpan start, TimeSpan end,TimeSpan time)
    {
        if (time.Days > 0) time = time.Subtract(new TimeSpan(time.Days,0,0,0)); // this may not be necessary??
        
        return !(time >= start && time <= end); 
    }
    
    public TimeSpan ConvertStrToTimespan(string timeOfDay)
    {
        return TimeSpan.Parse(timeOfDay);
    }

    public (TimeSpan, TimeSpan) CombineSchedules(List<TimeSpan> startSchedules, List<TimeSpan> endSchedules)
    {
        var minStart = startSchedules.Min();
        var maxEnd = endSchedules.Max();
        
        return (minStart, maxEnd);
    }

}

Combined schedule

Since we want reads to process as quickly as possible, it makes sense to hand the "determine the accurate combined schedule" step off to the refresh. It would be a separate step: after all the parking zone schedules have been updated, the program runs through each LPR and its associated parking zones. It may be worth making each LPR's value a dictionary with two keys, GUIDS and CombinedSchedule. The combined schedule is the maximum range across all the parking zones for that LPR, calculated on initialization/refresh.

Once this is set up, handling a read looks like this: receive the read, get the LPR unit name, look it up, and get the schedule for each day. A method on the LPR object picks which day range to use and checks whether the current time is in a valid range.
•      Saturday: it chooses the Saturday schedule, which is all day. That is a valid time, so return true.
•      Friday at 2 PM: it chooses the weekday schedule, which is 5:00 PM to 10:00 PM. The current time isn't in that range, so return false.

4. Refresh Service

The refresh service runs with Quartz. It has one ExecuteAsync with Quartz applied to it. Inside, it creates a Genetec HTTP client and a refresh method. The refresh method creates two instances of the same entity service (a service that retrieves entities from Genetec and returns them as the object types in our database), passing the client into each. The loaded objects are then saved to the database.

The reason for this structure is testability. We can test the service individually and, in a test environment, hand it a fake client that returns a forced value. This is vital, because right now we don't have access to Genetec and I can't test against it, but we still need to see whether our objects map correctly. It would be great to build a general structure and then plug in clients that simulate retrieving each entity.


Entity Service

The Refresh Service will use a entity service to retrieve entities from genetec. Genetec has a wrapper around the response, so this service allows an entity to be retrieved, and then mapped to a specified model type.

public class EntityService<T>(HttpClient httpClient) :IEntityService<T> where T: class // this is a genetec client that is passed in. Parameterized for testing
{
    public class Wrapper
    {
        public Rsp Rsp { get; set; }
    }

    public class Rsp
    {
        public string Status { get; set; }
        public T Result { get; set; }
    }


    public async Task<T> GetEntityAsync(string guid)
    {
        // I would like to learn about error handling and consider the best way to do it here. Before just randomly sprinkling try catch blocks all over
        
        HttpResponseMessage response;

        response = await httpClient.GetAsync($"entity/{guid}"); 

        var wrapper = await response.Content.ReadFromJsonAsync<Wrapper>(); 
        
        return wrapper.Rsp.Result; // this could fail if it was fed a not corresponding GUID. 
    }
    // can't get multiple entities at once with webSDK
}



Refresh Service

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


Why so many code snippets? Might as well just look at the repo! Anyways, I wanted to explain a few things that are going on. Above is a method found within refresh service. Since the other objects like permit- schedules or parking zone rules are just a go-between to get the information we want, we only have to store the refreshed parking zones. GetUpdatedParkingZones will retrieve the parking zones and their associated  configuration objects until you get the ScheduledPermits. And once you get the ScheduledPermits, you can get the start/end time for the Everyone schedule. And then it sets the associated property on the ParkingZone entity.

5. Exclusion Calendar with Athletic Events

Full excluded days go in a separate list that Quartz handles for when to toggle on/off ListenService .

The BYU calendar API is how we can get all the athletics events. Dates are in yyyy-MM-dd format:
https://calendar.byu.edu/api/Events?categories=10&event%5Bmin%5D%5Bdate%5D=2026-09-20&event%5Bmax%5D%5Bdate%5D=2026-12-28


6. Hangfire and Docker Setup

I created a new SQL Server container from the Microsoft SQL image and used it to create a database to configure and test Hangfire.
The issue I found. If the server in the connection string is set to localhost, it works well enough when you just run the application. For Docker, you need to change the server to sqlserver so the database can be accessed by other containers, and the runtime connection string should be stored in a secrets file. In the JetBrains data source, I need to connect through localhost instead, because JetBrains can reach localhost but the Docker container needs sqlserver as the server.
Example appsettings structure (secrets omitted):
{  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },  "AllowedHosts": "*",  "ApiUrls": {    "Paylock": "https://notrealpaylock.org",    "Textel": "https://foundation.textel.net/clientapi/v1",    "Genetec": "http://localhost:4590/WebSdk",    "Test": "http://localhost:8080/api"  },  "ConnectionStrings": {    "HangfireConnection": "Server=sqlserver,1433;Database=Hangfire;User Id=sa;Password=<from secrets>;TrustServerCertificate=True;"  }}
The container logs showed things going well: the Hangfire SQL objects were installed, and the Hangfire server started against SQL Server job storage with workers on the default queue.
 
