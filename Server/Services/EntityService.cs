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
    
    // VVV I actually don't think this is possible with the web SDK VVV
    
    /*public async Task<List<T>> GetEntities(List<string> guids)
    {
        String guidsQueryString = string.Join(",", guids);
        var response = await _httpClient.GetAsync($"entity/{guidsQueryString}");
        var obj = await response.Content.ReadFromJsonAsync<List<T>>();

       
        return obj;
    }*/
}