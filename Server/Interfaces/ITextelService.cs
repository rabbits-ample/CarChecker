
namespace Server;

public interface ITextelService
{
    public Task<HttpResponseMessage> sendTextAsync(string text, string phoneNumber);
}