namespace CarChecker_Real;

public interface ITextelService
{
    public Task sendTextAsync(string text, string phoneNumber);
}