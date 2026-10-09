namespace Server;

/// <summary>
/// Model for Permits -> From genetec, a description for a permitRestriciton object // Different than Permit objects in Genetec
/// </summary>
public class Permits:BaseClass
{
    /// <summary>
    /// A bool for if the schedule applies to all permits
    /// </summary>
    public bool IsAllPermits {get; set;}
    
    /// <summary>
    /// A bool for if the schedule applies to Everyone
    /// </summary>
    public bool IsEveryone  {get; set;}
    
    /// <summary>
    /// A bool for if the schedule applies to no permits
    /// </summary>
    public bool IsNoPermits {get; set;}
    
    /// <summary>
    /// A list of GUIDs to specific Permits objects in genetec
    /// </summary>
    public List<String> Guids {get; set;}
}