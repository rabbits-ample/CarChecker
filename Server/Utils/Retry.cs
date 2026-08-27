
namespace Server.Utils;

using System.Diagnostics.Contracts;


/// <summary>
/// Util for Retry behavior
/// </summary>
public static class Retry
{
    /// <summary>
    /// Executes a given asynchronous operation with a specified retry mechanism and exponential backoff behavior.
    /// </summary>
    public static async Task<HttpResponseMessage> Execute
        (Func<Task<HttpResponseMessage>> func, int maxAttempts = 3, int retryIntervalSeconds = 1, bool doExponentialBackoff = true)
    {
        Contract.Assert(maxAttempts > 0);
        Contract.Assert(retryIntervalSeconds > 0);

        List<Exception> exceptions = [];

        for (int i = 0; i < maxAttempts; i++)
        {
            try
            {
                if (i > 0)
                {
                    TimeSpan backoffSpan = TimeSpan.FromSeconds(doExponentialBackoff ? Math.Pow(2, i) : retryIntervalSeconds);
                    TimeSpan jitter = TimeSpan.FromMilliseconds(Random.Shared.Next(0, 250));
                    Console.WriteLine("retrying get token");
                    await Task.Delay(backoffSpan + jitter);
                }
                HttpResponseMessage resp = await func();
                resp.EnsureSuccessStatusCode();

                return resp;
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        }
        throw new AggregateException(exceptions);
    }
}