using Moq;
using Server;
using Server.Services;

namespace Test;

public class TestLotScheduleManager
{
    [Theory]
    [InlineData(16,50,0)]
    [InlineData(5,0,0)]
    [InlineData(12,30,30)]
    
    public void Convert_String_To_TimeSpan_Test(int hour, int min, int sec)
    {
        // Arrange
        LotScheduleManager manager = new LotScheduleManager();

        string timeStr = $"{hour}:{min}:{sec}";
        
        // Act
        TimeSpan time = manager.ConvertStrToTimespan(timeStr);
        
        // Assert
        var expected = new TimeSpan(hour,min,sec);
        Assert.Equal(expected, time);

    }

    [Theory]
    [InlineData(16, 50, 1, false)]
    [InlineData(3, 50, 0, true)]
    [InlineData(0, 00, 1, true)]
    [InlineData(16, 49, 0, true)]
    [InlineData(3, 50,0, true,1)]
    [InlineData(16, 50,1, false,1)]
    [InlineData(16, 50,1, false,200)]
    public void Is_Enforced_Now_Test(int hours, int min, int sec, bool result, int day = 0)

    {
        // Arrange
        LotScheduleManager manager = new LotScheduleManager();
        
        TimeSpan start = new TimeSpan(16, 50, 0); // 4:50 pm // an actual sample schedule from genetec
        TimeSpan end = new TimeSpan(23, 59, 00); // 11:59 pm

        ParkingZone parkingZone = new ParkingZone { StartEnforced = start, EndEnforced = end };

        
        TimeSpan now = day == 0 ? new TimeSpan(hours, min, sec) : new TimeSpan(day, hours, min, sec);
        
        // Act
        bool isEnforced = manager.IsEnforcedAt(parkingZone.StartEnforced, parkingZone.EndEnforced, now);

        // Assert
        Assert.Equal(result, isEnforced);
    }

    [Fact]
    public void Combined_Schedules_Test()
    {
        // Arrange
        LotScheduleManager manager = new LotScheduleManager();

        List<TimeSpan> startTimes = new()
        {
            new TimeSpan(12,0,0),
            new TimeSpan(16,30,0),
            new TimeSpan(11,25,0),
            new TimeSpan(12,30,0)
        };
        List<TimeSpan> endTimes = new()
        { 
            new TimeSpan(23,30,0),
            new TimeSpan(23,49,0),
            new TimeSpan(17,00,0),
            new TimeSpan(13,30,0)
        };
            
        // Act
        (TimeSpan combinedStart, TimeSpan combinedEnd) =  manager.CombineSchedules(startTimes, endTimes);
        
        //Assert
        Assert.Equal(startTimes[2], combinedStart);
        Assert.Equal(endTimes[1], combinedEnd);
        
    }
    
}