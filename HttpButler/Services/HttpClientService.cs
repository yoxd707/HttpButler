using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace HttpButler.Services;

/// <summary>
/// Servicio para realizar solicitudes HTTP utilizando un <see cref="IHttpClientFactory"/> configurable y resolución de rutas.
/// </summary>
/// <remarks> Este servicio simplifica el proceso de envío de solicitudes HTTP integrándose con <see
/// cref="IHttpClientFactory"/> para la creación de clientes y <see cref="IPathResolveService"/> para resolver URIs.
/// Este servicio soporta métodos HTTP comunes, y proporciona sobrecargas para manejar
/// respuestas con o sin deserialización. El servicio está diseñado para manejar escenarios donde se utilizan
/// múltiples clientes HTTP con nombre, y permite especificar una clave de fábrica para seleccionar el cliente.
/// Se admite el registro mediante un <see cref="ILogger"/> opcional para capturar errores y detalles de las peticiones.
/// </remarks>
public class HttpClientService : IHttpClientService
{
    private readonly IPathResolveService _pathResolveService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger? _logger;


    /// <summary>
    /// Inicializa una nueva instancia de <see cref="HttpClientService"/>.
    /// </summary>
    /// <param name="pathResolveService">Servicio de resolución de rutas.</param>
    /// <param name="httpClientFactory">Servicio de fabricación de clientes HTTP.</param>
    /// <param name="logger">Servicio opcional de registros y captura de errores.</param>
    public HttpClientService(IPathResolveService pathResolveService, IHttpClientFactory httpClientFactory, ILogger<HttpClientService>? logger = null)
    {
        _pathResolveService = pathResolveService;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }


    /// <summary>
    /// Envía una solicitud HTTP y devuelve la respuesta sin procesar.
    /// </summary>
    /// <param name="method">Método de la solicitud HTTP.</param>
    /// <param name="factoryKey">Clave para instanciar el cliente HTTP.</param>
    /// <param name="route">Ruta de la solicitud HTTP.</param>
    /// <param name="parameters">Parámetros para resolver la ruta.</param>
    /// <param name="body">Cuerpo de la solicitud HTTP, usado solamente en POST y PUT.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Respuesta de la solicitud HTTP sin procesar.</returns>
    private async Task<HttpResponseMessage> Send(HttpMethod method, string factoryKey, string route, object? parameters = null, object? body = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient(factoryKey);

            var uri = _pathResolveService.ResolveUri(route, parameters);
            var req = new HttpRequestMessage(method, uri);

            if (method == HttpMethod.Post || method == HttpMethod.Put)
            {
                using HttpContent content = body != null
                    ? JsonContent.Create(body)
                    : new StringContent(string.Empty);

                req.Content = content;
            }

            return await httpClient.SendAsync(req, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error occurred while making \"{Method}\" request to \"{Route}\" with parameters \"{Parameters}\" and body \"{Body}\"", method, route, parameters, body);
            throw;
        }
    }

    /// <summary>
    /// Envía una solicitud HTTP y deserializa el contenido de la respuesta en el tipo especificado,
    /// o devuelve <see langword="null"/> si el resultado no es exitoso.
    /// </summary>
    /// <typeparam name="T">Tipo al que se debe deserializar el contenido de la respuesta.</typeparam>
    /// <param name="method">Método de la solicitud HTTP.</param>
    /// <param name="factoryKey">Clave para instanciar el cliente HTTP.</param>
    /// <param name="route">Ruta de la solicitud HTTP.</param>
    /// <param name="parameters">Parámetros para resolver la ruta.</param>
    /// <param name="body">Cuerpo de la solicitud HTTP, usado solamente en POST y PUT.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Deserializa el contenido de la respuesta en el tipo <typeparamref name="T"/> si la respuesta es exitosa;
    /// <see langword="null"/> en caso contrario.</returns>
    private async Task<T?> SendWithNullableResult<T>(HttpMethod method, string factoryKey, string route, object? parameters = null, object? body = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await Send(method, factoryKey, route, parameters, body, cancellationToken);

            if (!response.IsSuccessStatusCode)
                return default;

            var result = await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error occurred while making \"{Method}\" request to \"{Route}\" with parameters \"{Parameters}\" and body \"{Body}\"", method, route, parameters, body);
            throw;
        }
    }

    public async Task Get(string factoryKey, string route, object? parameters = null, CancellationToken cancellationToken = default)
        => await Send(HttpMethod.Get, factoryKey, route, parameters, body: null, cancellationToken);

    public async Task<T> Get<T>(string factoryKey, string route, object? parameters = null, CancellationToken cancellationToken = default)
        => (await SendWithNullableResult<T>(HttpMethod.Get, factoryKey, route, parameters, body: null, cancellationToken)) ?? default!;

    public async Task<T?> GetWithNullableResult<T>(string factoryKey, string route, object? parameters = null, CancellationToken cancellationToken = default)
        => await SendWithNullableResult<T>(HttpMethod.Get, factoryKey, route, parameters, body: null, cancellationToken);


    public async Task Post(string factoryKey, string route, object? parameters = null, object? body = null, CancellationToken cancellationToken = default)
        => await Send(HttpMethod.Post, factoryKey, route, parameters, body, cancellationToken);

    public async Task<T> Post<T>(string factoryKey, string route, object? parameters = null, object? body = null, CancellationToken cancellationToken = default)
        => (await SendWithNullableResult<T>(HttpMethod.Post, factoryKey, route, parameters, body, cancellationToken)) ?? default!;

    public async Task<T?> PostWithNullableResult<T>(string factoryKey, string route, object? parameters = null, object? body = null, CancellationToken cancellationToken = default)
        => await SendWithNullableResult<T>(HttpMethod.Post, factoryKey, route, parameters, body, cancellationToken);


    public async Task Put(string factoryKey, string route, object? parameters = null, object? body = null, CancellationToken cancellationToken = default)
        => await Send(HttpMethod.Put, factoryKey, route, parameters, body, cancellationToken);

    public async Task<T> Put<T>(string factoryKey, string route, object? parameters = null, object? body = null, CancellationToken cancellationToken = default)
        => (await SendWithNullableResult<T>(HttpMethod.Put, factoryKey, route, parameters, body, cancellationToken)) ?? default!;

    public async Task<T?> PutWithNullableResult<T>(string factoryKey, string route, object? parameters = null, object? body = null, CancellationToken cancellationToken = default)
        => await SendWithNullableResult<T>(HttpMethod.Put, factoryKey, route, parameters, body, cancellationToken);


    public async Task Delete(string factoryKey, string route, object? parameters = null, CancellationToken cancellationToken = default)
        => await Send(HttpMethod.Delete, factoryKey, route, parameters, body: null, cancellationToken);

    public async Task<T> Delete<T>(string factoryKey, string route, object? parameters = null, CancellationToken cancellationToken = default)
        => (await SendWithNullableResult<T>(HttpMethod.Delete, factoryKey, route, parameters, body: null, cancellationToken)) ?? default!;

    public async Task<T?> DeleteWithNullableResult<T>(string factoryKey, string route, object? parameters = null, CancellationToken cancellationToken = default)
        => await SendWithNullableResult<T>(HttpMethod.Delete, factoryKey, route, parameters, body: null, cancellationToken);

}
