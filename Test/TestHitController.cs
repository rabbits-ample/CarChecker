using System.Net;
using System.Net.Http.Json;
using Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Xunit;

namespace Test;

public class TestHitController
{
    [Fact]
    public async Task HitController_Sends_Warning_Text_If_Car_Is_Registered_But_Not_Active()
    {
        var paylockMock = new Mock<IPaylockService>();
        paylockMock
            .Setup(m => m.GetCarInfoAsync(It.IsAny<string>()))
            .ReturnsAsync(new Car { Registered = true, Active = false, Owner = "Dave", PhoneNumber = "1234567890"});

        var textelMock = new Mock<ITextelService>();
        textelMock
            .Setup(m => m.sendTextAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(HttpStatusCode.OK);
        var hitController = new HitController(paylockMock.Object,textelMock.Object);
        var result = (OkObjectResult)(await hitController.ReceiveHit("LicensePlate#"));
        Assert.Equal(result.Value, "Warning text was sent" );
    }
    [Fact]
    public async Task HitController_Returns_Not_Found_When_PaylockService_Returns_Null()
    {
        Car? nullCar = null;
        var paylockMock = new Mock<IPaylockService>();
        paylockMock
            .Setup(m => m.GetCarInfoAsync(It.IsAny<string>()))
            .ReturnsAsync(nullCar);
        
        var textelMock = new Mock<ITextelService>();
        textelMock
            .Setup(m => m.sendTextAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(HttpStatusCode.OK);
        var hitController = new HitController(paylockMock.Object,textelMock.Object);
        
        var result =(NotFoundObjectResult)(await hitController.ReceiveHit("NOT_IN_DATABASE"));
        Assert.Equal(result.Value, "Car with license plate # 'NOT_IN_DATABASE' not found." );
    }
}