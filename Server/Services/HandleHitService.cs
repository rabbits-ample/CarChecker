namespace Server.Services;
public class HandleHitService : IHandleHitService
{
// depending on whether or not we can receive to a specific endpoint, we might have to
// change this controller so that it instead is a background process that initiates and calls a method.
    private readonly IPaylockService _paylockService;
    
    private readonly ITextelService _textelService;
    public HandleHitService( IPaylockService paylockService, ITextelService textelService)
    {
   
        _paylockService = paylockService;
        _textelService = textelService;
    }
    
    public async Task ReceiveHit(string plate)
    {
        Car car =  await _paylockService.GetCarInfoAsync(plate); // lookup plate, return Car object
      
       if (car.Registered == true)
           if (car.Active == false)
           {
               // if opted in
               var warningText = $"Dear {car.Owner}, your car is about to explode."; 
               await _textelService.sendTextAsync(warningText,car.PhoneNumber);
       
           }
    }

}
