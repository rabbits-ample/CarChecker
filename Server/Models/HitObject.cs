namespace Server;
/// <summary>
/// Hit object, received from Genetic streaming
/// </summary>
public class HitObject
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