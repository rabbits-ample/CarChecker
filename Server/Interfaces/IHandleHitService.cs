namespace Server.Services;

public interface IHandleHitService
{
    public Task ReceiveHit(string plate, bool test = false);
}