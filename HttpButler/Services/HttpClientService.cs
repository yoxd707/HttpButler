using System.Net.Http.Json;

namespace HttpButler.Services;

public class HttpClientService : IHttpClientService
{
    private readonly IPathResolveService _pathResolveService;
    private readonly IHttpClientFactory _httpClientFactory;

    public HttpClientService(IPathResolveService pathResolveService, IHttpClientFactory httpClientFactory)
    {
        _pathResolveService = pathResolveService;
        _httpClientFactory = httpClientFactory;
    }

    public async Task Get(string factoryKey, string route, object? parameters = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient(factoryKey);

            var uri = _pathResolveService.ResolveUri(route, parameters);

            var response = await httpClient.GetAsync(uri, cancellationToken);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<T> Get<T>(string factoryKey, string route, object? parameters = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient(factoryKey);

            var uri = _pathResolveService.ResolveUri(route, parameters);

            var response = await httpClient.GetAsync(uri, cancellationToken);

            if (!response.IsSuccessStatusCode)
                return default!;

            var result = await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
            return result ?? default!;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<T?> GetWithNullableResult<T>(string factoryKey, string route, object? parameters = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient(factoryKey);

            var uri = _pathResolveService.ResolveUri(route, parameters);

            var response = await httpClient.GetAsync(uri, cancellationToken);

            if (!response.IsSuccessStatusCode)
                return default;

            var result = await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

}
