using Server;

namespace Test;

public class TestParkingMapDatabase
{

    [Theory]
    [InlineData(6)]
    [InlineData(20)]
    [InlineData(2)]
    [InlineData(1)]
    public void ParkingMapDatabase_Can_Save_And_Retrieve_Multiple_Objects(int num)
    {
        // Arrange
        ParkingMapDatabase parkingMapDatabase = new ParkingMapDatabase();
        Random random = new();
        string guid = "";
        string zoneName = "";
        // Act
        int zoneToTest = random.Next(0,num);
        
        for (int i = 0; i < num; i++)
        {
            ParkingZone parkingZone = new();
            parkingZone.Name = $"Zone{i}";
            parkingMapDatabase.Save(parkingZone);
            
            if (i == zoneToTest)
            {
                guid = parkingZone.Id;
                zoneName = parkingZone.Name;
            }
        }
        // Assert
        var retrievedObj = parkingMapDatabase.GetById<ParkingZone>(guid);
        Assert.Equal(retrievedObj.Name, zoneName);
        
    }
    
    [Fact]
    public void ParkingMapDatabase_Save_Updates_Obj_When_Obj_Already_Exists_In_Table() // this is a lie. Because everything is a reference. it doesn't need to be saved. It just alters the actual obj in the table
    {
        // Arrange
        ParkingMapDatabase parkingMapDatabase = new ParkingMapDatabase();
        ParkingZone parkingZone = new ParkingZone{Name = "Zone 1"};

        // Act
        String newZoneName = "Zone 5";
        
        parkingMapDatabase.Save(parkingZone);
        
        parkingZone.Name = newZoneName; // technically this would not be changed in a refresh. but it's the general idea
        
        parkingMapDatabase.Save(parkingZone); // you don't even need to save for it to save changes. 
        
        // Assert
        String id = parkingZone.Id;
        ParkingZone? retrievedParkingZone = parkingMapDatabase.GetById<ParkingZone>(id);
        
        Assert.NotNull(retrievedParkingZone);
        Assert.Equal(newZoneName, retrievedParkingZone.Name);
        
    }
    
    [Fact]
    public void ParkingMapDatabase_Saves_Child_Objects()
    {
        // Arrange
        bool testBool = true;
        string testString = "Weekdays";
        
        ParkingMapDatabase parkingMapDatabase = new ParkingMapDatabase();
        
        List<Permits> permits = new() { new Permits {IsAllPermits =  testBool} };
        List<ScheduledPermit> schedules = new() { new ScheduledPermit {Days = testString} };
        
        PermitRestriction permitRestriction = new PermitRestriction{ ScheduledPermits = schedules};

        string id = permitRestriction.Id;

        // Act
        parkingMapDatabase.Save(permitRestriction);
        
        // Assert
        var retrievedPermitRestriction = parkingMapDatabase.GetById<PermitRestriction>(id);
        Assert.NotEmpty(retrievedPermitRestriction.ScheduledPermits);
        
        Assert.Equal(testString, permitRestriction.ScheduledPermits[0].Days);
        
        // So there is an important distinction. Right now, the database will not save child objects in there own table.
        // For now it's just going to save a reference in memory.
        
        // You can't retreive a saved child objects by id unless you explicitly save it by id as well.
        // It's possible to configure it so it saves recursively, but for what we need, we probably don't have to
        
    }
    // test errors too 
}