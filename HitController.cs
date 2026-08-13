using Microsoft.AspNetCore.Mvc;

namespace CarChecker_Real;
[Route("api")]
[ApiController]
public class HitController: ControllerBase
{
    private readonly Token _token;
    private readonly IPlateLookupService _plateLookupService;
    private readonly IRetrieveTokenService _retrieveTokenService;
    public HitController(Token token, IPlateLookupService plateLookupService, IRetrieveTokenService retrieveTokenService)
    {
        _token = token;
        _plateLookupService = plateLookupService;
        _retrieveTokenService = retrieveTokenService;
    }

    [HttpPost("{licensePlateNumber}")]
    public async Task<IActionResult> ReceiveHit(string licensePlateNumber)
    {
        _token.TestThis = licensePlateNumber;
        _retrieveTokenService.RefreshToken();
        Console.WriteLine($"This is the token now: {_token.TestThis}");
        return Ok();
        // lookupPlate using _plateLookupService.lookupPlate(licensePlateNumber)
        // call send text which processes the returned object.
    }

}
