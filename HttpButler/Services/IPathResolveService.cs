namespace HttpButler.Services;

/// <summary>
/// Servicio para resolver rutas con parámetros en URIs.
/// </summary>
public interface IPathResolveService
{
    /// <summary>
    /// Resuelve una URI a partir de una ruta y parámetros opcionales.
    /// </summary>
    /// <typeparam name="T">Tipo del cual se tomará las propiedades como parámetros.</typeparam>
    /// <param name="path">Ruta a resolver.</param>
    /// <param name="parameters">Instancia de la cual se usarán las propiedades como parámetros.</param>
    /// <returns><see cref="Uri"/> de la ruta con los parámetros incluidos.</returns>
    Uri ResolveUri<T>(string path, T? parameters = default)
        where T : class;
}