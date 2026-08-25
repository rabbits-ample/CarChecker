using System.Dynamic;
using System.Text.Json;

namespace Server.Services;

public class ListenService:BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private HttpClient _httpClient;

    public ListenService(IServiceScopeFactory scopeFactory)
    {
       _scopeFactory = scopeFactory;
    }
    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _httpClient = new HttpClient();
        //_httpClient.BaseAddress = new Uri("http://localhost:4590/WebSdk/");
        _httpClient.BaseAddress = new Uri("http://host.docker.internal:5101/api/events");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConnectAndReadAsync(stoppingToken);
            }catch (Exception exception) when (!stoppingToken.IsCancellationRequested )
            {
                Console.WriteLine($"Stream dropped: {exception.Message}. Reconnecting...");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); // backoff
            }
        }
    }

    public async Task ConnectAndReadAsync(CancellationToken cancellationToken)
    {
        var scope = _scopeFactory.CreateScope();
        IHandleHitService handleHitServiceService = scope.ServiceProvider.GetRequiredService<IHandleHitService>();
        
        Console.WriteLine("Connecting to the server...");
        
        var subscriptions = await _httpClient.GetAsync("events/subscribed");
        if (subscriptions.Content == null)
        {
            await _httpClient.GetAsync("events/subscribe?q=event(LprUnit,{eventType})");
            // you can get event type raise by an entity/ maybe entityTYpe (so do that on LprUnit/ get an LprUnit id and then get it's event types to know which event type we want
        }
        var response = await _httpClient.GetAsync("",HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);
        
        while (!cancellationToken.IsCancellationRequested)
        {
            var awaitLimitInSeconds = 10;
            var expiration =  Task.Delay(awaitLimitInSeconds*1000);
            var lineResponse = reader.ReadLineAsync();

            await Task.WhenAny(lineResponse, expiration);
            if (expiration.IsCompleted)
            {
                throw new Exception($"Stream has been silent for more than {awaitLimitInSeconds} seconds");
            }
            var line = lineResponse.Result;
            if (!string.IsNullOrWhiteSpace(line))
            {
                var hitObject = JsonSerializer.Deserialize<HitObject>(line);
                handleHitServiceService.ReceiveHit(hitObject.Plate,true);
            }
            
           
        }
    }
}