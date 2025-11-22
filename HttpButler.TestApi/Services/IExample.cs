using HttpButler.Attributes;

namespace HttpButler.TestApi.Services;

[HttpButler]
//[Route("v1/example")]
public interface IExample
{
    [HttpGet]
    [Route("ping")]
    public Task PingAsync();

    [HttpGet]
    public Task<string> GetHelloAsync();

    //[HttpGet("{name}")]
    //public Task<string> GetHelloAsync(string name);

    //[HttpGet("{name}/{photoId}")]
    //public Task<string?> GetHelloAsync(string name, int photoId = 0);

}
