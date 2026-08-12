using Microsoft.AspNetCore.Mvc;

namespace CarChecker_Real;
[Route("api")]
[ApiController]
public class HitController: ControllerBase
{
    public HitController()
    {
        
    }

    [HttpPost("{licensePlateNumber}")]
    public async Task<IActionResult> ReceiveHit(string licensePlateNumber)
    {
        Console.WriteLine(licensePlateNumber);
        return Ok();
    }

}
