using System.Net;

namespace CarChecker_Real;

public interface ITextelService
{
    public Task<HttpStatusCode> sendTextAsync(string text, string phoneNumber);
}