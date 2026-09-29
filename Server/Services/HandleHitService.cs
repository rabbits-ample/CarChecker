namespace Server.Services;
public class HandleHitService : IHandleHitService
{
// depending on whether or not we can receive to a specific endpoint, we might have to
// change this controller so that it instead is a background process that initiates and calls a method.
    private readonly IPaylockService _paylockService;
    
    public HandleHitService( IPaylockService paylockService)
    {
   
        _paylockService = paylockService;
    }

    public async Task ReceiveHit(string plate)
    { 
        // logic to check if in enforced schedule
        
        await _paylockService.GetCarInfoAsync(plate);
           // directly after this the program needs to return because then the job won't retry. 
           // The more time there is after a text is sent, the more chance there is that a job gets retried, 
    }

}
