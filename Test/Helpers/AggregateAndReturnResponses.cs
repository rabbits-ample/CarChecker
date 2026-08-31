using Server.Services;

namespace Test.Helpers;

public class AggregateAndReturnResponses : IHandleHitService
{
    public List<String> QueuedReads = new ();
    public List<String> ProcessedReads = new();
    private readonly int _delay; 

    public AggregateAndReturnResponses(int delay = 3000)
    {
        _delay = delay;
    }

    public async Task ReceiveHit(string plate)
    {
        QueuedReads.Add(plate);
        //Console.WriteLine(plate);
        var queued2Text = "Queued Reads: " + string.Join(", ", QueuedReads);
        Console.WriteLine(queued2Text);
        await Task.Delay(_delay);
        Console.WriteLine($"{plate} processed");
        QueuedReads.Remove(plate);
        ProcessedReads.Add(plate);
        
        // Depending on how fast events come in, and how long it takes to process, we could have up to around 30 -50 queues events.
        // If the connection breaks, would there be a good way to store queued events?
    }

}