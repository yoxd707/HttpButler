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
        => (await GetWithNullableResult<T>(factoryKey, route, parameters, cancellationToken)) ?? default!;

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

    public async Task Post(string factoryKey, string route, object? parameters = null, object? body = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient(factoryKey);

            var uri = _pathResolveService.ResolveUri(route, parameters);

            using HttpContent httpContent = body != null
                ? JsonContent.Create(body)
                : new StringContent(string.Empty);

            var response = await httpClient.PostAsync(uri, httpContent, cancellationToken);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<T> Post<T>(string factoryKey, string route, object? parameters = null, object? body = null, CancellationToken cancellationToken = default)
        => (await PostWithNullableResult<T>(factoryKey, route, parameters, body, cancellationToken)) ?? default!;

    public async Task<T?> PostWithNullableResult<T>(string factoryKey, string route, object? parameters = null, object? body = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient(factoryKey);

            var uri = _pathResolveService.ResolveUri(route, parameters);

            using HttpContent httpContent = body != null
                ? JsonContent.Create(body)
                : new StringContent(string.Empty);

            var response = await httpClient.PostAsync(uri, httpContent, cancellationToken);

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


    public async Task Put(string factoryKey, string route, object? parameters = null, object? body = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient(factoryKey);

            var uri = _pathResolveService.ResolveUri(route, parameters);

            using HttpContent httpContent = body != null
                ? JsonContent.Create(body)
                : new StringContent(string.Empty);

            var response = await httpClient.PutAsync(uri, httpContent, cancellationToken);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<T> Put<T>(string factoryKey, string route, object? parameters = null, object? body = null, CancellationToken cancellationToken = default)
        => (await PutWithNullableResult<T>(factoryKey, route, parameters, body, cancellationToken)) ?? default!;

    public async Task<T?> PutWithNullableResult<T>(string factoryKey, string route, object? parameters = null, object? body = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient(factoryKey);

            var uri = _pathResolveService.ResolveUri(route, parameters);

            using HttpContent httpContent = body != null
                ? JsonContent.Create(body)
                : new StringContent(string.Empty);

            var response = await httpClient.PutAsync(uri, httpContent, cancellationToken);

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


    public async Task Delete(string factoryKey, string route, object? parameters = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient(factoryKey);

            var uri = _pathResolveService.ResolveUri(route, parameters);

            var response = await httpClient.DeleteAsync(uri, cancellationToken);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<T> Delete<T>(string factoryKey, string route, object? parameters = null, CancellationToken cancellationToken = default)
        => (await DeleteWithNullableResult<T>(factoryKey, route, parameters, cancellationToken)) ?? default!;

    public async Task<T?> DeleteWithNullableResult<T>(string factoryKey, string route, object? parameters = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient(factoryKey);

            var uri = _pathResolveService.ResolveUri(route, parameters);

            var response = await httpClient.DeleteAsync(uri, cancellationToken);

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
