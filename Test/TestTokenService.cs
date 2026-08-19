using System.Net;
using System.Net.Http.Json;
using Server;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Xunit;

namespace Test;

public class TestTokenService
{
    public HttpClient createMockHttpClient(HttpStatusCode statusCode, object? content = null)
    {
        // Set up a mock HttpMessageHandler to control the HttpClient's behavior.
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
    public void IsTokenValid_Returns_False_If_Token_Is_Expired(int expirationInSeconds)
    {
        // Arrange
        var token = new Token { AccessToken = "mocked-token", ExpiresIn = 3600, RetrievedAt =  (DateTime.Now).AddSeconds(-expirationInSeconds) }; 
        var httpClient = createMockHttpClient(HttpStatusCode.OK, token);
        var tokenShelf = new TokenShelf();
        // Act
        var service = new TokenService(httpClient, tokenShelf);
        var isValid = service.IsTokenValid(token);
        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public async Task GetTokenAsync_Raises_Error_If_Response_Is_Not_Success()
    {
        // Arrange
        var mockResponse = new Token { AccessToken = "mocked-token" };
        var httpClient = createMockHttpClient(HttpStatusCode.Forbidden, mockResponse);
        var tokenShelf = new TokenShelf();
        var testClientId = "test-client-id";
        var testClientSecret = "test-client-secret";
        var path = "fake/path";
        var content = new StringContent("Test");
        // Act 
        var service = new TokenService(httpClient, tokenShelf);
        // Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => service.GetTokenAsync(path, content));
        Assert.Equal("Response status code does not indicate success: 403 (Forbidden).", exception.Message);
    }

    [Fact]
    public void IsTokenValidReturnsFalseIfAccessTokenIsEmpty()
    {
        // Arrange
        var token = new Token { AccessToken = "", ExpiresIn = 3600, RetrievedAt = DateTime.Now }; 
        var httpClient = createMockHttpClient(HttpStatusCode.OK, token);
        var tokenShelf = new TokenShelf();
        // Act
        var service = new TokenService(httpClient, tokenShelf);
        var isValid = service.IsTokenValid(token);
        // Assert
        Assert.False(isValid);
    }

    [Theory]
    [InlineData("mocked_token")]
    [InlineData(null)]
    public async Task GetTokenAsync_Returns_Token(string? expectedToken)
    {
        // Arrange
        var mockResponse = new Token { AccessToken = expectedToken };
        var httpClient = createMockHttpClient(HttpStatusCode.OK, mockResponse);
        var tokenShelf = new TokenShelf();
        var testClientId = "test-client-id";
        var testClientSecret = "test-client-secret";
        var path = "fake/path";
        var content = new StringContent("Test");
        // Act
        var service = new TokenService(httpClient, tokenShelf);
        var token = await service.GetTokenAsync(path, content);
        // Assert
        Assert.Equal(expectedToken, token.AccessToken);
        tokenShelf.Tokens.TryGetValue(content, out Token dictToken);
        Assert.Equal(dictToken.AccessToken, token.AccessToken);
    }
}
