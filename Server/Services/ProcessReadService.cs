namespace Server.Services;
public class ProcessReadService : IProcessReadService
{
    private readonly IPaylockService _paylockService;
    
    public ProcessReadService( IPaylockService paylockService)
    {
   
        _paylockService = paylockService;
    }

    public async Task ProcessReadAsync(string plate)
    { 
        // logic to check if in enforced schedule
        
        await _paylockService.GetCarInfoAsync(plate);
           // directly after this the program needs to return because then the job won't retry. 
           // The more time there is after a text is sent, the more chance there is that a job gets retried, 
    }

}
