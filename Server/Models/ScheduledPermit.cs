namespace Server;
/// <summary>
/// ScheduledPermit object that represents a ParkingZone's schedule
/// </summary>
public class ScheduledPermit : BaseClass
{
    /// <summary>
    /// String that contains the enforced days of a schedules
    /// </summary>
    public string Days {get; set;}
    
    /// <summary>
    /// String of the EndHour of enforcement within a day
    /// </summary>
    public string EndHour {get; set;}
    
    /// <summary>
    /// String of the StartHour of enforcement within a day
    /// </summary>
    public string StartHour {get; set;}
    
    public Permits Permits {get; set;}
}