using System.Net;
using Server;
using Moq;
using Server.Services;
namespace Test;

public class TestHandleHitService
{
    [Fact]
    public async Task HitController_Sends_Warning_Text_If_Car_Is_Registered_But_Not_Active()
    {
        // Arrange
        var paylockMock = new Mock<IPaylockService>();
        paylockMock
            .Setup(m => m.GetCarInfoAsync(It.IsAny<string>()))
            .ReturnsAsync(new Car { Registered = true, Active = false, Owner = "Dave", PhoneNumber = "1234567890"});

        var textelMock = new Mock<ITextelService>();
        textelMock
            .Setup(m => m.sendTextAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new HttpResponseMessage{StatusCode = HttpStatusCode.OK});
        var handleHitService = new HandleHitService(paylockMock.Object,textelMock.Object);
        
       
        // Act
        using (var stringWriter = new StringWriter())
        {
            Console.SetOut(stringWriter);
            await handleHitService.ReceiveHit("LicensePlate#", false);
            var output = stringWriter.ToString();
        
        // Assert
            
            Assert.Equal("Warning text was sent\n",output);
        }
    
        
        
    }
}