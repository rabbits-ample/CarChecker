using System.Net;

namespace Server;

public interface ITextelService
{
    public Task<HttpStatusCode> sendTextAsync(string text, string phoneNumber);
}