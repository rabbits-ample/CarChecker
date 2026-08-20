namespace Server.Services;

public interface IHandleHitService
{
    public void ReceiveHit(string plate,bool test = false);
}