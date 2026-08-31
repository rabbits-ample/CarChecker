using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Moq;
using Server.Services;
using Test.Helpers;

namespace Test;

public class TestListenService: IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private WebApplicationFactory<Program> _newFactory;
    private readonly int _testDuration = 5000;

    public TestListenService(WebApplicationFactory<Program> factory)
    {
       _factory = factory;
    }

    public ListenService CreateLiveListenService(IHandleHitService hitServiceStrategy)
    {
        var fakeHandleHitService = hitServiceStrategy;

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
        
        _newFactory = _factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");
            b.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(IHandleHitService));
                services.RemoveAll(typeof(IHostedService));
            });
        });
        
        var realClient = _newFactory.CreateClient();
        
        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        mockHttpClientFactory
            .Setup(f => f.CreateClient("Genetec"))
            .Returns(realClient);
      
        ListenService listenService = new ListenService(mockScopeFactory.Object, mockHttpClientFactory.Object);
        return listenService;
    }

    [Theory]
    [InlineData( 1000)]
    [InlineData( 2000)]
    [InlineData( 5000)]
    public async Task ListenService_Processes_And_Queues_Reads_Within_An_Expected_Range(int delay)
    { 
        // Arrange
        AggregateAndReturnResponses hitServiceStrategy = new AggregateAndReturnResponses(delay);
        var listenService = CreateLiveListenService(hitServiceStrategy);
        var cts = new CancellationTokenSource();
        
        //Act
        await listenService.StartAsync(cts.Token);
        await Task.Delay(_testDuration);
        
        //Assert
        
        // how many started before 5,000 - _testDuration ( 5,000 -  3,300 = 1,700 / 200 = 8.5 (round up I supposed)
        int totalReads = _testDuration / 200;
        var expectedProcessed = Math.Floor((decimal)((_testDuration - delay) / 200));
        var expectedQueued = totalReads - expectedProcessed;
        
        
        var actualQueuedReads = hitServiceStrategy.QueuedReads;
        var actualProcessedReads = hitServiceStrategy.ProcessedReads;

        Assert.NotEmpty(actualQueuedReads);
        
        var intersects = actualQueuedReads.Intersect(actualProcessedReads);
        Assert.Empty(intersects);
        
        Assert.InRange(actualQueuedReads.Count, expectedQueued-1, expectedQueued+1);
        Assert.InRange(actualProcessedReads.Count, expectedProcessed-1, expectedProcessed+1);
       
    }
    [Fact]
    public async Task ListenService_Handles_Multiple_Instances_Of_ReceiveHit()
    {
        // Arrange
        AggregateAndReturnResponses hitServiceStrategy = new AggregateAndReturnResponses();
        var listenService = CreateLiveListenService(hitServiceStrategy);
        var cts = new CancellationTokenSource();
        
        // Act
        await listenService.StartAsync(cts.Token);
        await Task.Delay(_testDuration);
        
        // Assert
        var actualQueuedReads = hitServiceStrategy.QueuedReads;
        Assert.NotEqual(1,actualQueuedReads.Count);
    }
}