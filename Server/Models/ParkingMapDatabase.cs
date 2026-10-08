using System.Reflection.Metadata;

namespace Server;

/// <summary>
/// A simulated database class that will hold all the objects in memory
/// </summary>


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