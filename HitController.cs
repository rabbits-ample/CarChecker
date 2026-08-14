using Microsoft.AspNetCore.Mvc;

namespace CarChecker_Real;
[Route("api")]
[ApiController]
public class HitController: ControllerBase
{
    private Token _token;
    private readonly IPlateLookupService _plateLookupService;
    private readonly ITokenService _tokenService;
    private readonly ITextelService _textelService;
    public HitController(Token token, IPlateLookupService plateLookupService, ITokenService tokenService, ITextelService textelService)
    {
        _token = token;
        _plateLookupService = plateLookupService;
        _tokenService = tokenService;
        _textelService = textelService;
    }

    [HttpPost("{licensePlateNumber}")]
    public async Task<IActionResult> ReceiveHit(string licensePlateNumber)
    {
       _token = await _tokenService.GetTokenAsync(_token);
       Car car =  await _plateLookupService.GetCarInfoAsync(licensePlateNumber,_token.AccessToken); // lookup plate, return Car object
      if (car == null)
       {
            throw new Exception("Car not found");
       }
      
       if (car.Registered == true)
           if (car.Active == false)
           {
               // if opted in
               var warningText = $"Dear {car.Owner}, your car is about to explode.";
               await _textelService.sendTextAsync(warningText,car.PhoneNumber);
           }

       // else, do nothing
       return Ok();
        
        
     
        
    }

}
