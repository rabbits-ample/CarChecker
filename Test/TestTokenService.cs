using System.Net;
using System.Net.Http.Json;
using CarChecker_Real;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Xunit;

namespace Test;

public class TestTokenService
{
    public HttpClient createMockHttpClient(HttpStatusCode statusCode, object? content = null)
    {
        // Setup a mock HttpMessageHandler to control the HttpClient's behavior.
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), // Matches any HttpRequestMessage
                ItExpr.IsAny<CancellationToken>())  // Matches any CancellationToken
            .ReturnsAsync(new HttpResponseMessage // Returns a successful HTTP response
            {
                StatusCode = statusCode,
                Content = content is not null ? JsonContent.Create(content) : null // Content serialized as JSON
            });

        // Create an HttpClient instance using the mocked handler.
        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        httpClient.BaseAddress = new Uri("https://fake.token.endpoint");
        return httpClient;
    }
    [Theory]
    [InlineData(3540)] //The exact age at which it expires (60 second buffer)
    [InlineData(3600)]
    [InlineData(3700)]
    [InlineData(10000)]
    public void IsTokenValidReturnsFalseIfTokenIsExpired(int expirationInSeconds)
    {
        
        var token = new Token { AccessToken = "mocked-token", ExpiresIn = 3600, RetrievedAt =  (DateTime.Now).AddSeconds(-expirationInSeconds) }; 
        var httpClient = createMockHttpClient(HttpStatusCode.OK, token);
        var tokenShelf = new TokenShelf();
        // Create an instance of the service with the mocked dependencies.
        var service = new TokenService(httpClient, tokenShelf);
        var isValid = service.IsTokenValid(token);
        Assert.False(isValid);
    }

    [Fact]
    public async Task GetTokenAsyncRaisesErrorIfResponseIsNotSuccess()
    {
        var mockResponse = new Token { AccessToken = "mocked-token" };
        var httpClient = createMockHttpClient(HttpStatusCode.Forbidden, mockResponse);
        var tokenShelf = new TokenShelf();
        // Create an instance of the service with the mocked dependencies.
        var service = new TokenService(httpClient, tokenShelf);
        var testClientId = "test-client-id";
        var testClientSecret = "test-client-secret";
        // Call the method under test to get the token.
        
        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => service.GetTokenAsync(testClientId, testClientSecret));
        Assert.Equal("Response status code does not indicate success: 403 (Forbidden).", exception.Message);
    }

    [Fact]
    public void IsTokenValidReturnsFalseIfAccessTokenIsEmpty()
    {
        
        var token = new Token { AccessToken = "", ExpiresIn = 3600, RetrievedAt = DateTime.Now }; 
        var httpClient = createMockHttpClient(HttpStatusCode.OK, token);
        var tokenShelf = new TokenShelf();
        // Create an instance of the service with the mocked dependencies.
        var service = new TokenService(httpClient, tokenShelf);
        var isValid = service.IsTokenValid(token);
        Assert.False(isValid);
    }

    [Theory]
    [InlineData("mocked_token")]
    [InlineData(null)]
    public async Task GetTokenAsyncReturnsTokenThatMatchesExpectedToken(string? expectedToken)
    {
        var mockResponse = new Token { AccessToken = expectedToken };
        var httpClient = createMockHttpClient(HttpStatusCode.OK, mockResponse);
        var tokenShelf = new TokenShelf();
        // Create an instance of the service with the mocked dependencies.
        var service = new TokenService(httpClient, tokenShelf);
        var testClientId = "test-client-id";
        var testClientSecret = "test-client-secret";
        // Call the method under test to get the token.
        var token = await service.GetTokenAsync(testClientId, testClientSecret);

        // Verify that the returned token matches the expected mocked token.
        Assert.Equal(expectedToken, token.AccessToken);
        tokenShelf.Tokens.TryGetValue($"{testClientId},{testClientSecret}", out Token dictToken);
        Assert.Equal(dictToken.AccessToken, token.AccessToken);
    }
}
