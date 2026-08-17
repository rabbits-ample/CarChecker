namespace Server;
/// <summary>
/// Token Shelf contains the tokens for this project
/// </summary>
public class TokenShelf
{
    /// <summary>
    /// A dictionary where the key is the configuration key, and token is the value
    /// </summary>
    public Dictionary<string, Token> Tokens = new Dictionary<string, Token>();
}