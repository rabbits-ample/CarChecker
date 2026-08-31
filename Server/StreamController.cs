using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
namespace StreamAPITest;


[ApiController]
public class StreamController : ControllerBase
{

    [HttpGet("events")]
    public async Task openStream(CancellationToken cancellationToken)
    {
        Response.ContentType = "text/event-stream";

        int counter = 0;
        while (!cancellationToken.IsCancellationRequested)
        {
            //var line = $"{{\"Plate\":\"{plates[counter]}\",\"Ingress\": false }}\n";
            
            var json = new
            {
                Plate =  $"Plate{counter}",
                Ingress =  false,
            };
            var content = $"{JsonSerializer.Serialize(json)}\n";
            
            await Response.WriteAsync(content, cancellationToken);
            
            await Response.Body.FlushAsync(cancellationToken);
            
            //var delay = (int)(delays[counter] * 1000);
            // don't change this anymore
            var delay = 200;
            await Task.Delay(delay, cancellationToken);
            counter++;
        }
    }

    [HttpGet("events/subscribed")]
    public async Task<IActionResult> Subscribed()
    {
        return Ok();
    }
}