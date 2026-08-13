namespace CarChecker_Real;
using System.Text.Json.Serialization;
public class Token
{
/// <summary>
/// Object to store the response from getting a token
/// </summary>
    /// <summary>
    /// The received access token
    /// </summary>
    /// Whatever the heck the JSON property name is from the received TOKEN response.
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    /// <summary>
    /// How long before the token expires
    /// </summary>
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    /// <summary>
    /// The type of token
    /// </summary>
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;

    public string TestThis { get; set; } 

}