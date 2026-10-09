using System.Text.Json;
using Hangfire;

namespace Server.Services;

public class ListenService:BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private HttpClient _httpClient;

    public ListenService(IServiceScopeFactory scopeFactory,IHttpClientFactory httpClientFactory)
    {
       _scopeFactory = scopeFactory;
       _httpClient = httpClientFactory.CreateClient("Genetec");
    }
    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //"http://host.docker.internal:5101/api/events" // uri for separate server
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
        IProcessReadService processReadServiceService = scope.ServiceProvider.GetRequiredService<IProcessReadService>();
        
        Console.WriteLine("Connecting to the server...");
        
        
        var subscriptions = await _httpClient.GetAsync("events/subscribed");
        if (subscriptions.Content == null)
        {
            await _httpClient.GetAsync("events/subscribe?q=event(LprUnit,{eventType})");
            //events/subscribe?q=event(GUID,Read),event(GUID,Read)
            // can do GUID instead of entityName
        }
        var response = await _httpClient.GetAsync("events",HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);
        
        while (!cancellationToken.IsCancellationRequested)
        {
            var awaitLimitInSeconds = 5;
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
                var hitObject = JsonSerializer.Deserialize<Read>(line);
                
                BackgroundJob.Enqueue(() => processReadServiceService.ProcessReadAsync(hitObject.Plate));

            }
            
           
        }
    }
}