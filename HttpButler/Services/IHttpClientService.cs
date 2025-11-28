namespace HttpButler.Services;

/// <summary>
/// Servicio para realizar solicitudes HTTP.
/// </summary>
/// TODO: Agregar soporte para otros métodos HTTP (PATCH, HEAD, OPTIONS, etc.)
/// TODO: Agregar soporte para configuración de encabezados personalizados, autenticación, etc.
/// TODO: Agregar manejo de errores y reintentos.
/// TODO: La factoryKey obliga a la implementación a manejar múltiples clientes HTTP configurados de manera diferente.
public interface IHttpClientService
{

    /// <summary>
    /// Envía una solicitud HTTP GET.
    /// </summary>
    /// <param name="factoryKey">Clave para instanciar el cliente HTTP.</param>
    /// <param name="route">Ruta de la solicitud HTTP.</param>
    /// <param name="parameters">Parámetros para resolver la ruta.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    Task Get(string factoryKey, string route, object? parameters = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Envía una solicitud HTTP GET y deserializa el contenido de la respuesta en el tipo especificado,
    /// o devuelve <see langword="default"/> si el resultado no es exitoso.
    /// </summary>
    /// <typeparam name="T">Tipo al que se debe deserializar el contenido de la respuesta.</typeparam>
    /// <param name="factoryKey">Clave para instanciar el cliente HTTP.</param>
    /// <param name="route">Ruta de la solicitud HTTP.</param>
    /// <param name="parameters">Parámetros para resolver la ruta.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Deserializa el contenido de la respuesta en el tipo <typeparamref name="T"/> si la respuesta es exitosa;
    /// <see langword="default"/> en caso contrario.</returns>
    Task<T> Get<T>(string factoryKey, string route, object? parameters = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Envía una solicitud HTTP GET y deserializa el contenido de la respuesta en el tipo especificado,
    /// o devuelve <see langword="null"/> si el resultado no es exitoso.
    /// </summary>
    /// <typeparam name="T">Tipo al que se debe deserializar el contenido de la respuesta.</typeparam>
    /// <param name="factoryKey">Clave para instanciar el cliente HTTP.</param>
    /// <param name="route">Ruta de la solicitud HTTP.</param>
    /// <param name="parameters">Parámetros para resolver la ruta.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Deserializa el contenido de la respuesta en el tipo <typeparamref name="T"/> si la respuesta es exitosa;
    /// <see langword="null"/> en caso contrario.</returns>
    Task<T?> GetWithNullableResult<T>(string factoryKey, string route, object? parameters = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Envía una solicitud HTTP POST.
    /// </summary>
    /// <param name="factoryKey">Clave para instanciar el cliente HTTP.</param>
    /// <param name="route">Ruta de la solicitud HTTP.</param>
    /// <param name="parameters">Parámetros para resolver la ruta.</param>
    /// <param name="body">Cuerpo de la solicitud HTTP.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    Task Post(string factoryKey, string route, object? parameters = null, object? body = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Envía una solicitud HTTP POST y deserializa el contenido de la respuesta en el tipo especificado,
    /// o devuelve <see langword="default"/> si el resultado no es exitoso.
    /// </summary>
    /// <typeparam name="T">Tipo al que se debe deserializar el contenido de la respuesta.</typeparam>
    /// <param name="factoryKey">Clave para instanciar el cliente HTTP.</param>
    /// <param name="route">Ruta de la solicitud HTTP.</param>
    /// <param name="parameters">Parámetros para resolver la ruta.</param>
    /// <param name="body">Cuerpo de la solicitud HTTP.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Deserializa el contenido de la respuesta en el tipo <typeparamref name="T"/> si la respuesta es exitosa;
    /// <see langword="default"/> en caso contrario.</returns>
    Task<T> Post<T>(string factoryKey, string route, object? parameters = null, object? body = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Envía una solicitud HTTP POST y deserializa el contenido de la respuesta en el tipo especificado,
    /// o devuelve <see langword="null"/> si el resultado no es exitoso.
    /// </summary>
    /// <typeparam name="T">Tipo al que se debe deserializar el contenido de la respuesta.</typeparam>
    /// <param name="factoryKey">Clave para instanciar el cliente HTTP.</param>
    /// <param name="route">Ruta de la solicitud HTTP.</param>
    /// <param name="parameters">Parámetros para resolver la ruta.</param>
    /// <param name="body">Cuerpo de la solicitud HTTP.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Deserializa el contenido de la respuesta en el tipo <typeparamref name="T"/> si la respuesta es exitosa;
    /// <see langword="null"/> en caso contrario.</returns>
    Task<T?> PostWithNullableResult<T>(string factoryKey, string route, object? parameters = null, object? body = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Envía una solicitud HTTP PUT.
    /// </summary>
    /// <param name="factoryKey">Clave para instanciar el cliente HTTP.</param>
    /// <param name="route">Ruta de la solicitud HTTP.</param>
    /// <param name="parameters">Parámetros para resolver la ruta.</param>
    /// <param name="body">Cuerpo de la solicitud HTTP.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    Task Put(string factoryKey, string route, object? parameters = null, object? body = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Envía una solicitud HTTP PUT y deserializa el contenido de la respuesta en el tipo especificado,
    /// o devuelve <see langword="default"/> si el resultado no es exitoso.
    /// </summary>
    /// <typeparam name="T">Tipo al que se debe deserializar el contenido de la respuesta.</typeparam>
    /// <param name="factoryKey">Clave para instanciar el cliente HTTP.</param>
    /// <param name="route">Ruta de la solicitud HTTP.</param>
    /// <param name="parameters">Parámetros para resolver la ruta.</param>
    /// <param name="body">Cuerpo de la solicitud HTTP.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Deserializa el contenido de la respuesta en el tipo <typeparamref name="T"/> si la respuesta es exitosa;
    /// <see langword="default"/> en caso contrario.</returns>
    Task<T> Put<T>(string factoryKey, string route, object? parameters = null, object? body = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Envía una solicitud HTTP PUT y deserializa el contenido de la respuesta en el tipo especificado,
    /// o devuelve <see langword="null"/> si el resultado no es exitoso.
    /// </summary>
    /// <typeparam name="T">Tipo al que se debe deserializar el contenido de la respuesta.</typeparam>
    /// <param name="factoryKey">Clave para instanciar el cliente HTTP.</param>
    /// <param name="route">Ruta de la solicitud HTTP.</param>
    /// <param name="parameters">Parámetros para resolver la ruta.</param>
    /// <param name="body">Cuerpo de la solicitud HTTP.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Deserializa el contenido de la respuesta en el tipo <typeparamref name="T"/> si la respuesta es exitosa;
    /// <see langword="null"/> en caso contrario.</returns>
    Task<T?> PutWithNullableResult<T>(string factoryKey, string route, object? parameters = null, object? body = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Envía una solicitud HTTP DELETE.
    /// </summary>
    /// <param name="factoryKey">Clave para instanciar el cliente HTTP.</param>
    /// <param name="route">Ruta de la solicitud HTTP.</param>
    /// <param name="parameters">Parámetros para resolver la ruta.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    Task Delete(string factoryKey, string route, object? parameters = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Envía una solicitud HTTP DELETE y deserializa el contenido de la respuesta en el tipo especificado,
    /// o devuelve <see langword="default"/> si el resultado no es exitoso.
    /// </summary>
    /// <typeparam name="T">Tipo al que se debe deserializar el contenido de la respuesta.</typeparam>
    /// <param name="factoryKey">Clave para instanciar el cliente HTTP.</param>
    /// <param name="route">Ruta de la solicitud HTTP.</param>
    /// <param name="parameters">Parámetros para resolver la ruta.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Deserializa el contenido de la respuesta en el tipo <typeparamref name="T"/> si la respuesta es exitosa;
    /// <see langword="default"/> en caso contrario.</returns>
    Task<T> Delete<T>(string factoryKey, string route, object? parameters = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Envía una solicitud HTTP DELETE y deserializa el contenido de la respuesta en el tipo especificado,
    /// o devuelve <see langword="null"/> si el resultado no es exitoso.
    /// </summary>
    /// <typeparam name="T">Tipo al que se debe deserializar el contenido de la respuesta.</typeparam>
    /// <param name="factoryKey">Clave para instanciar el cliente HTTP.</param>
    /// <param name="route">Ruta de la solicitud HTTP.</param>
    /// <param name="parameters">Parámetros para resolver la ruta.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Deserializa el contenido de la respuesta en el tipo <typeparamref name="T"/> si la respuesta es exitosa;
    /// <see langword="null"/> en caso contrario.</returns>
    Task<T?> DeleteWithNullableResult<T>(string factoryKey, string route, object? parameters = null, CancellationToken cancellationToken = default);

}
