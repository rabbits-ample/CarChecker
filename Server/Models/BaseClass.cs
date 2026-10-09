namespace Server;
/// <summary>
/// Base Class object that all database objects inherit from
/// </summary>
public class BaseClass
{
    /// <summary>
    /// Unique Id for all classes that inherit
    /// </summary>
    public string Id { get; set; } =  Guid.NewGuid().ToString();
}