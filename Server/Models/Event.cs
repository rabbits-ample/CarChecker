namespace Server;
/// <summary>
/// Model for an event object from BYU's Calendar API
/// </summary>
public class Event
{
    /// <summary>
    /// Title of the event
    /// </summary>
    public String Title { get; set; }
    
    /// <summary>
    /// Description of the event
    /// </summary>
    public String Description { get; set; }
    
    /// <summary>
    /// Start date of the event
    /// </summary>
    public String StartDateTime { get; set; }
}