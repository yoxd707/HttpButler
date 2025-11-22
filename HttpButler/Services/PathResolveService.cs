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

        PropertyCache<T>.SetupPropertyCache(parameters);
        var props = PropertyCache<T>.Properties;

        int lastIndex = 0;

        // Construcción de la ruta base sustituyendo parámetros.
        foreach (var matchParam in routeInfo.Params)
        {
            // Se agrega el segmento de ruta previo al parámetro.
            sb.Append(pathSpan.Slice(lastIndex, matchParam.RouteIndex - lastIndex).ToArray());

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
            sb.Append(pathSpan.Slice(lastIndex).ToArray());

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
                else if (sb[sb.Length - 1] != '&' && sb[sb.Length - 1] != '?')
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
        where T : class
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
    private readonly struct MatchParam
    {
        public string Name { get; }
        public ParamType Type { get; }
        public int RouteIndex { get; }

        public MatchParam(string name, ParamType type, int routeIndex)
        {
            Name = name;
            Type = type;
            RouteIndex = routeIndex;
        }
    }

    /// <summary>
    /// Información de la ruta y sus parámetros.
    /// </summary>
    /// <param name="QueryStartIndex">Índice en el que inicia la query, -1 si no existe.</param>
    /// <param name="Params">Colección de coincidencias de parámetros en la ruta.</param>
    private readonly struct RouteInfo
    {
        public int QueryStartIndex { get; }
        public MatchParam[] Params { get; }

        public RouteInfo(int queryStartIndex, MatchParam[] @params)
        {
            QueryStartIndex = queryStartIndex;
            Params = @params;
        }
    }

    /// <summary>
    /// Propiedad cacheada para acceso rápido al valor.
    /// </summary>
    /// <typeparam name="T">Tipo del cual se cacheó la propiedad.</typeparam>
    /// <param name="Name">Nombre de la propiedad.</param>
    /// <param name="Getter">Función para obetener valor de la propiedad.</param>
    private readonly struct CachedProperty<T>
        where T : class
    {
        public string Name { get; }
        public Func<T, object?> Getter { get; }

        public CachedProperty(string name, Func<T, object?> getter)
        {
            Name = name;
            Getter = getter;
        }
    }

    /// <summary>
    /// Cache estática de propiedades por tipo.
    /// </summary>
    /// <typeparam name="T">Tipo del cual se cachean las propiedades.</typeparam>
    private static class PropertyCache<T>
        where T : class
    {
        private static CachedProperty<T>[]? properties;
        public static CachedProperty<T>[] Properties => properties ?? new CachedProperty<T>[0];

        public static void SetupPropertyCache(T obj)
        {
            if (properties is not null) return;

            var objType = obj.GetType();
            var props = objType.GetProperties();
            var tempProperties = new CachedProperty<T>[props.Length];

            for (int i = 0; i < props.Length; i++)
            {
                var p = props[i];
                var getterMethod = p.GetGetMethod();
                if (getterMethod is null) continue;

                var instanceParam = System.Linq.Expressions.Expression.Parameter(typeof(T), "instance");
                var castedInstance = System.Linq.Expressions.Expression.Convert(instanceParam, objType);
                var propertyAccess = System.Linq.Expressions.Expression.Call(castedInstance, getterMethod);
                var castToObject = System.Linq.Expressions.Expression.Convert(propertyAccess, typeof(object));
                var lambda = System.Linq.Expressions.Expression.Lambda<Func<T, object?>>(castToObject, instanceParam);
                var getter = lambda.Compile();

                tempProperties[i] = new CachedProperty<T>(p.Name, getter);
            }

            properties = tempProperties;
        }
    }


}
