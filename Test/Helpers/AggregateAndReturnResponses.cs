using Server.Services;

namespace Test.Helpers;

public class AggregateAndReturnResponses : IHandleHitService
{
    public async Task ReceiveHit(string plate)
    {
        Console.WriteLine(plate);
        //await Task.Delay(50);
    }

}