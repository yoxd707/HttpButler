namespace HttpButler.Services;

internal interface IHttpClientService
{

    Task Get(string factoryKey, string route, object? parameters = null, CancellationToken cancellationToken = default);
    Task<T?> Get<T>(string factoryKey, string route, object? parameters = null, CancellationToken cancellationToken = default);

}
