namespace Server;
/// <summary>
/// Read received from Genetic 
/// </summary>
public class Read
{
    /// <summary>
    /// plate
    /// </summary>
    public string Plate { get; set; } = null!;
    /// <summary>
    /// Whether or not a hit object was ingress (This attribute doesn't exist in the payload :(
    /// </summary>
    public bool Ingress { get; set; }
   
}