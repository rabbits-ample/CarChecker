namespace Server;

/// <summary>
/// Parking Zone obj representing entity in Genetec
/// </summary>
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