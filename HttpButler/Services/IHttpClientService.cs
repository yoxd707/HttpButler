namespace HttpButler.Services;

public interface IHttpClientService
{

    Task Get(string factoryKey, string route, object? parameters = null, CancellationToken cancellationToken = default);
    Task<T> Get<T>(string factoryKey, string route, object? parameters = null, CancellationToken cancellationToken = default);
    Task<T?> GetWithNullableResult<T>(string factoryKey, string route, object? parameters = null, CancellationToken cancellationToken = default);

    Task Post(string factoryKey, string route, object? parameters = null, object? body = null, CancellationToken cancellationToken = default);
    Task<T> Post<T>(string factoryKey, string route, object? parameters = null, object? body = null, CancellationToken cancellationToken = default);
    Task<T?> PostWithNullableResult<T>(string factoryKey, string route, object? parameters = null, object? body = null, CancellationToken cancellationToken = default);

    Task Put(string factoryKey, string route, object? parameters = null, object? body = null, CancellationToken cancellationToken = default);
    Task<T> Put<T>(string factoryKey, string route, object? parameters = null, object? body = null, CancellationToken cancellationToken = default);
    Task<T?> PutWithNullableResult<T>(string factoryKey, string route, object? parameters = null, object? body = null, CancellationToken cancellationToken = default);

    Task Delete(string factoryKey, string route, object? parameters = null, CancellationToken cancellationToken = default);
    Task<T> Delete<T>(string factoryKey, string route, object? parameters = null, CancellationToken cancellationToken = default);
    Task<T?> DeleteWithNullableResult<T>(string factoryKey, string route, object? parameters = null, CancellationToken cancellationToken = default);

}
