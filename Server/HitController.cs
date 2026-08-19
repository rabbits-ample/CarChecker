using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace Server;
[Route("api")]
[ApiController]
public class HitController: ControllerBase
{
// depending on whether or not we can receive to a specific endpoint, we might have to
// change this controller so that it instead is a background process that initiates and calls a method.
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
            return NotFound($"Car with license plate # '{licensePlateNumber}' not found.");
       }
      
       if (car.Registered == true)
           if (car.Active == false)
           {
               // if opted in
               var warningText = $"Dear {car.Owner}, your car is about to explode.";
               var result = await _textelService.sendTextAsync(warningText,car.PhoneNumber);
               if (!(result == HttpStatusCode.OK))
               {
                   return StatusCode(502, "Failed to send warning text");
               }
               return Ok("Warning text was sent");
           }

       // else, do nothing
       return Ok();
        
        
     
        
    }

}
