using System.Dynamic;
using System.Text.Json;

namespace Server.Services;

public class ListenService:BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ListenService(IServiceScopeFactory scopeFactory)
    {
       _scopeFactory = scopeFactory;
    }
    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConnectAndReadAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
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
        var client = new HttpClient();
        //var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:4590/WebSdk/events");
        var request = new HttpRequestMessage(HttpMethod.Get, "http://host.docker.internal:5101/api/events");
        var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);
        // Find a way to catch silence. Because this code technically only triggers on receive, we might have to do an external thing
        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync();
            var hitObject = JsonSerializer.Deserialize<HitObject>(line);
            // one chunk = one line (newline-delimited)
            if (string.IsNullOrWhiteSpace(line))
            {
                handleHitServiceService.ReceiveHit(hitObject.Plate, true);
            }
            
           
        }
    }
}