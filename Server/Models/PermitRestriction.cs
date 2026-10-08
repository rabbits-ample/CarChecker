namespace Server;

/// <summary>
/// Model for a PermitRestriction object from genetec
/// </summary>
public class PermitRestriction:BaseClass
{
    /// <summary>
    /// List of associated schedules based off each permit, as defined in Genetec
    /// </summary>
    public List<ScheduledPermit> ScheduledPermits {get; set;}
    
}