using Server.Interfaces;

namespace Server.Services;

public class EntityService<T>(HttpClient httpClient) :IEntityService<T> where T: class // this is a genetec client that is passed in. Parameterized for testing
{
    public class Wrapper
    {
        public Rsp Rsp { get; set; }
    }

    public class Rsp
    {
        public string Status { get; set; }
        public T Result { get; set; }
    }


    public async Task<T> GetEntityAsync(string guid)
    {
        // I would like to learn about error handling and consider the best way to do it here. Before just randomly sprinkling try catch blocks all over
        
        HttpResponseMessage response;

        response = await httpClient.GetAsync($"entity/{guid}"); 

        var wrapper = await response.Content.ReadFromJsonAsync<Wrapper>(); 
        
        return wrapper.Rsp.Result; // this could fail if it was fed a not corresponding GUID. 
    }
    // can't get multiple entities at once with webSDK
}