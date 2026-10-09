using System.Net;
using System.Net.Http.Json;
using Moq;
using Moq.Protected;

namespace Test.Helpers;

public class CreateMockClient(HttpStatusCode statusCode,  string path, object? content = null)
{
    public HttpClient HttpClient = CreateMockHttpClient(statusCode, path, content);
    
    private static  HttpClient CreateMockHttpClient(HttpStatusCode statusCode,  string path, object? content = null) 
    {
        // Set up a mock HttpMessageHandler to control the HttpClient's behavior.
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.Method == HttpMethod.Get &&
                    r.RequestUri.AbsolutePath.EndsWith(path)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage // Returns a successful HTTP response
            {
                StatusCode = statusCode,
                Content = JsonContent.Create(content) // Create is used on an object! not a string.
            });

        // Create an HttpClient instance using the mocked handler.
        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        httpClient.BaseAddress = new Uri("https://fakeurl");
        return httpClient;
    }
}