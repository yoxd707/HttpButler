using HttpButler.Attributes;

namespace HttpButler.TestApi.Services;

[HttpButler]
[Route("v1/example")]
public interface IExample
{
    [HttpGet]
    [Route("ping")]
    public Task PingAsync();                        // Route: v1/example/ping

    [HttpGet]
    public Task<string> GetHelloAsync();            // Route: v1/example

    [HttpGet("{name}")]
    public Task<string> GetHelloAsync(string name); // Route: v1/example/{name}

    [HttpGet("{name}/{photoId}")]
    public Task<string> GetHelloAsync(string name, int photoId = 0); // Route: v1/example/{name}/{photoId}
}
