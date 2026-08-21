namespace Server;
/// <summary>
/// Car object, data received from paylock will be mapped to this
/// </summary>
public class Car
{
    /// <summary>
    /// Plate of given car
    /// </summary>
    public string Plate { get; set; } = null!;
    /// <summary>
    /// Model of car
    /// </summary>
    public string? Model { get; set; }
    /// <summary>
    /// Owner of car
    /// </summary>
    public string? Owner { get; set; }
    /// <summary>
    /// Active status of car's permits
    /// </summary>
    public bool? Active { get; set; }
    /// <summary>
    /// Registered status of car
    /// </summary>
    public bool? Registered { get; set; }
    /// <summary>
    /// Owner's phone number
    /// </summary>
    public string? PhoneNumber { get; set; }
}