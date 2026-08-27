using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
namespace StreamAPITest;


[Route("api")]
[ApiController]
public class StreamController : ControllerBase
{

    [HttpGet("events")]
    public async Task openStream(CancellationToken cancellationToken)
    {
        Response.ContentType = "text/event-stream";

        int counter = 0;
        List<int> delays = new List<int> { 1, 1, 1, 1, 1, 1, 1, 1};
        List<string> plates = new List<string>() { "2C-5837", "1A-7429", "4B-3168", "7D-9052", "3F-6814", "8H-4276", "5K-1938", "9M-8641" };

        while (!cancellationToken.IsCancellationRequested)
        {
            //var line = $"{{\"Plate\":\"{plates[counter]}\",\"Ingress\": false }}\n";
            
            var json = new
            {
                Plate =  plates[counter],
                Ingress =  false,
            };
            var content = $"{JsonSerializer.Serialize(json)}\n";
            
            await Response.WriteAsync(content, cancellationToken);
            
            await Response.Body.FlushAsync(cancellationToken);
            
            var delay = (int)(delays[counter] * 1000);
            await Task.Delay(delay, cancellationToken);
            counter++;
            if (counter >= delays.Count)
                counter = 0;
        }
    }

    [HttpGet("events/subscribed")]
    public async Task<IActionResult> Subscribed()
    {
        return Ok();
    }
}