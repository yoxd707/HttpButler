using System.Collections.Concurrent;
using System.Text;

namespace HttpButler.Services;

/// <summary>
/// Servicio para resolver rutas con parámetros en URIs.
/// </summary>
public class PathResolveService : IPathResolveService
{
    private static readonly ConcurrentDictionary<string, RouteInfo> _routeCache = new();

    /// <summary>
    /// Resuelve una URI a partir de una ruta y parámetros opcionales.
    /// </summary>
    /// <typeparam name="T">Tipo del cual se tomará las propiedades como parámetros.</typeparam>
    /// <param name="path">Ruta a resolver.</param>
    /// <param name="parameters">Instancia de la cual se usarán las propiedades como parámetros.</param>
    /// <returns><see cref="Uri"/> de la ruta con los parámetros incluidos.</returns>
    /// <exception cref="HttpRouteResolveException"></exception>
    public Uri ResolveUri<T>(string path, T? parameters = null)
        where T : class
    {
        if (parameters is null)
            return new Uri(path, UriKind.RelativeOrAbsolute);

        if (!_routeCache.TryGetValue(path, out var routeInfo))
        {
            routeInfo = GetRouteInfo(path);
            _routeCache.TryAdd(path, routeInfo);
        }

        var sb = new StringBuilder(256);
        var pathSpan = path.AsSpan();
        
        var props = PropertyCache<T>.Properties;

        int lastIndex = 0;

        // Construcción de la ruta base sustituyendo parámetros.
        foreach (var matchParam in routeInfo.Params)
        {
            // Se agrega el segmento de ruta previo al parámetro.
            sb.Append(pathSpan.Slice(lastIndex, matchParam.RouteIndex - lastIndex));

            // Se busca el parámetro en el objeto.
            if (TryGetPropertyValue(props, parameters, matchParam.Name, out var valueStr))
            {
                if (matchParam.Type == ParamType.Query)
                {
                    sb.Append(matchParam.Name)
                        .Append('=')
                        .Append(valueStr);
                }
                else
                {
                    sb.Append(valueStr);
                }

                // Saltamos el placeholder en la ruta original (+2 por las llaves {}): {nombre}
                lastIndex = matchParam.RouteIndex + matchParam.Name.Length + 2;
            }
            else
            {
                var message = $"The value for parameter '{matchParam.Name}' could not be found, path to resolve: '{path}'.";
                throw new HttpRouteResolveException(message);
            }
        }

        // Se agrega el resto de la ruta.
        if (lastIndex < path.Length)
            sb.Append(pathSpan.Slice(lastIndex));

        // Manejo de Query Params adicionales (propiedades que no estaban en la ruta).
        bool hasQueryInBuilder = routeInfo.QueryStartIndex >= 0;

        foreach (var prop in props)
        {
            // Ignoramos la propiedad si ya fue usada como parámetro de ruta.
            if (IsParamUsedInRoute(routeInfo.Params, prop.Name))
                continue;

            var val = prop.Getter(parameters);
            if (val is null) continue;

            var valStr = Uri.EscapeDataString(val.ToString() ?? string.Empty);

            if (sb.Length > 0)
            {
                // Lógica para decidir si añadir ? o &
                if (!hasQueryInBuilder)
                {
                    sb.Append('?');
                    hasQueryInBuilder = true;
                }
                else if (sb[^1] != '&' && sb[^1] != '?')
                {
                    sb.Append('&');
                }
            }

            sb.Append(prop.Name)
                .Append('=')
                .Append(valStr);
        }

        return new Uri(sb.ToString(), UriKind.RelativeOrAbsolute);
    }

    private static bool IsParamUsedInRoute(MatchParam[] paramsList, string propName)
    {
        foreach (var p in paramsList)
        {
            if (p.Name.Equals(propName, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    private static bool TryGetPropertyValue<T>(CachedProperty<T>[] props, T instance, string name, out string? value)
    {
        foreach (var prop in props)
        {
            if (prop.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                var val = prop.Getter(instance);
                value = val != null ? Uri.EscapeDataString(val.ToString() ?? string.Empty) : string.Empty;
                return true;
            }
        }
        value = null;
        return false;
    }

    /// <summary>
    /// Analiza la ruta para extraer información sobre los parámetros.
    /// </summary>
    /// <param name="path">Ruta a analizar.</param>
    /// <returns><see cref="RouteInfo"/> con la información de los parámetros.</returns>
    private static RouteInfo GetRouteInfo(string path)
    {
        var matchParams = new List<MatchParam>();

        int queryStartIndex = path.IndexOf('?');
        int i = 0;

        while (i < path.Length)
        {
            int start = path.IndexOf('{', i);
            if (start == -1) break;

            int end = path.IndexOf('}', start);
            if (end == -1) break;

            string name = path.Substring(start + 1, end - start - 1);
            bool isQuery = queryStartIndex >= 0 && start > queryStartIndex;

            matchParams.Add(new MatchParam(name, isQuery ? ParamType.Query : ParamType.Route, start));

            i = end + 1;
        }

        return new RouteInfo(queryStartIndex, matchParams.ToArray());
    }

    /// <summary>
    /// Tipo de parámetro en la ruta.
    /// </summary>
    private enum ParamType
    {
        Route,
        Query
    }

    /// <summary>
    /// Información de un parámetro coincidente en la ruta.
    /// </summary>
    /// <param name="Name">Nombre del parámetro.</param>
    /// <param name="Type">Tipo de parámetro.</param>
    /// <param name="RouteIndex">Índice en que el parámetro inicia sobre la ruta.</param>
    private readonly record struct MatchParam(string Name, ParamType Type, int RouteIndex);

    /// <summary>
    /// Información de la ruta y sus parámetros.
    /// </summary>
    /// <param name="QueryStartIndex">Índice en el que inicia la query, -1 si no existe.</param>
    /// <param name="Params">Colección de coincidencias de parámetros en la ruta.</param>
    private readonly record struct RouteInfo(int QueryStartIndex, MatchParam[] Params);

    /// <summary>
    /// Propiedad cacheada para acceso rápido al valor.
    /// </summary>
    /// <typeparam name="T">Tipo del cual se cacheó la propiedad.</typeparam>
    /// <param name="Name">Nombre de la propiedad.</param>
    /// <param name="Getter">Función para obetener valor de la propiedad.</param>
    private readonly record struct CachedProperty<T>(string Name, Func<T, object?> Getter);

    /// <summary>
    /// Cache estática de propiedades por tipo.
    /// </summary>
    /// <typeparam name="T">Tipo del cual se cachean las propiedades.</typeparam>
    private static class PropertyCache<T>
    {
        public static readonly CachedProperty<T>[] Properties;

        static PropertyCache()
        {
            var props = typeof(T).GetProperties();
            Properties = new CachedProperty<T>[props.Length];

            for (int i = 0; i < props.Length; i++)
            {
                var p = props[i];

                var instanceParam = System.Linq.Expressions.Expression.Parameter(typeof(T), "instance");
                var propertyAccess = System.Linq.Expressions.Expression.Property(instanceParam, p);
                var castToObject = System.Linq.Expressions.Expression.Convert(propertyAccess, typeof(object));
                var lambda = System.Linq.Expressions.Expression.Lambda<Func<T, object?>>(castToObject, instanceParam);
                var getter = lambda.Compile();

                Properties[i] = new CachedProperty<T>(p.Name, getter);
            }
        }
    }


}
