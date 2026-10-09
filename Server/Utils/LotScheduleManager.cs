namespace Server.Services;

public class LotScheduleManager
{
    
    public bool IsEnforcedNow(TimeSpan start, TimeSpan end)
    {
        TimeSpan now = DateTime.Now.TimeOfDay;
        return IsEnforcedAt(start, end, now);
    }
    
    public bool IsEnforcedAt(TimeSpan start, TimeSpan end,TimeSpan time)
    {
        if (time.Days > 0) time = time.Subtract(new TimeSpan(time.Days,0,0,0)); // this may not be necessary??
        
        return !(time >= start && time <= end); 
    }
    
    public TimeSpan ConvertStrToTimespan(string timeOfDay)
    {
        return TimeSpan.Parse(timeOfDay);
    }

    public (TimeSpan, TimeSpan) CombineSchedules(List<TimeSpan> startSchedules, List<TimeSpan> endSchedules)
    {
        var minStart = startSchedules.Min();
        var maxEnd = endSchedules.Max();
        
        return (minStart, maxEnd);
    }

}