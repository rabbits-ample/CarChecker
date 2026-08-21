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
              //  await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
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
        // Find a way to catch silence. Because this code technically only triggers on receive, we might have to do an external thing
        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line =  await reader.ReadLineAsync();
            if (!string.IsNullOrWhiteSpace(line))
            {
                var hitObject = JsonSerializer.Deserialize<HitObject>(line);
                handleHitServiceService.ReceiveHit(hitObject.Plate);
            }
            
           
        }
    }
}