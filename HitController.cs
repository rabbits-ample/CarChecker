using Microsoft.AspNetCore.Mvc;

namespace CarChecker_Real;
[Route("api")]
[ApiController]
public class HitController: ControllerBase
{
    private readonly Token _token;
    private readonly IPlateLookupService _plateLookupService;
    public HitController(Token token, IPlateLookupService plateLookupService)
    {
        _token = token;
        _plateLookupService = plateLookupService;
    }

    [HttpPost("{licensePlateNumber}")]
    public async Task<IActionResult> ReceiveHit(string licensePlateNumber)
    {
        _token.TestThis = licensePlateNumber;
        Console.WriteLine($"This is the token now: {_token.TestThis}");
        return Ok();
    }

}
