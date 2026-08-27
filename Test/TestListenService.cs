using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Server.Services;
using Test.Helpers;

namespace Test;

public class TestListenService
{
    

    public TestListenService()
    {
    
    }

  
    [Fact]
    public async Task TestListen()
    {

        var fakeHandleHitService = new AggregateAndReturnResponses();

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IHandleHitService)))
            .Returns(fakeHandleHitService);

        var mockScope = new Mock<IServiceScope>();
        mockScope
            .Setup(s => s.ServiceProvider)
            .Returns(mockServiceProvider.Object);

        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        mockScopeFactory
            .Setup(sf => sf.CreateScope())
            .Returns(mockScope.Object);
        
        


        var mockedhttpClientFactory = new Mock<IHttpClientFactory>();
        
        ListenService listenService = new ListenService(mockScopeFactory.Object, mockedhttpClientFactory.Object);
        var cts = new CancellationTokenSource();
        await listenService.StartAsync(cts.Token);
  
        
    }
}