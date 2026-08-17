using Microsoft.AspNetCore.Mvc;

namespace CarChecker_Real;
[Route("api")]
[ApiController]
public class HitController: ControllerBase
{

    private readonly IPaylockService _paylockService;
    
    private readonly ITextelService _textelService;
    public HitController( IPaylockService paylockService, ITextelService textelService)
    {
   
        _paylockService = paylockService;
        _textelService = textelService;
    }

    [HttpPost("{licensePlateNumber}")]
    public async Task<IActionResult> ReceiveHit(string licensePlateNumber)
    {
       Car car =  await _paylockService.GetCarInfoAsync(licensePlateNumber); // lookup plate, return Car object
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
