using HttpButler.Utils;
using System.Collections.Concurrent;
using System.Text;

namespace HttpButler.Services;

/// <summary>
/// Servicio para resolver rutas con parámetros en URIs.
/// </summary>
public class PathResolveService : IPathResolveService
{
    private static readonly ConcurrentDictionary<Guid, RouteInfo> cache = new();

    /// <summary>
    /// Resuelve una URI a partir de una ruta y parámetros opcionales.
    /// </summary>
    /// <param name="path">Ruta a resolver.</param>
    /// <param name="parameters">Instancia de la cual se usarán las propiedades como parámetros.</param>
    /// <returns>URI de la ruta con los parámetros incluidos.</returns>
    public Uri ResolveUri(string path, object? parameters = null)
    {
        var route = path;

        if (parameters is not null)
        {
            var cacheKey = DeterministicGuid.FromString(path);
            var routeInfo = cache.GetOrAdd(cacheKey, key => GetRouteInfo(path));
            var matchParams = routeInfo.Params;

            var sbRoute = new StringBuilder(256);
            var sbQueryParams = new StringBuilder(256);

            var properties = parameters.GetType().GetProperties();

            var matchParamsCount = matchParams.Length;
            var matchParamIndex = 0;
            var lastMatchParamRouteIndex = 0;

            MatchParam matchParam = matchParamIndex < matchParamsCount
                ? matchParams[matchParamIndex]
                : default;

            foreach (var prop in properties)
            {
                var propName = prop.Name;
                var propValue = Uri.EscapeDataString(prop.GetValue(parameters)!.ToString()!);

                if (matchParamIndex < matchParamsCount && matchParam.Name.Equals(propName, StringComparison.OrdinalIgnoreCase))
                {
                    matchParamIndex++;

                    var start = lastMatchParamRouteIndex;
                    var end = matchParam.RouteIndex;

                    sbRoute.Append(path[start..end]);

                    if (matchParam.Type == ParamType.Query)
                        sbRoute.Append(propName)
                            .Append('=');

                    sbRoute.Append(propValue);

                    lastMatchParamRouteIndex = end + matchParam.Name.Length + 2;

                    if (matchParamIndex < matchParamsCount)
                        matchParam = matchParams[matchParamIndex];
                }
                else
                {
                    if (sbQueryParams.Length == 0)
                    {
                        if (routeInfo.QueryStartIndex < 0)
                            sbQueryParams.Append('?');
                        else if (path[^1] != '&')
                            sbQueryParams.Append('&');
                    }
                    else
                        sbQueryParams.Append('&');

                    sbQueryParams
                        .Append(prop.Name)
                        .Append('=')
                        .Append(propValue);
                }
            }

            if (path.Length > lastMatchParamRouteIndex)
                sbRoute.Append(path[lastMatchParamRouteIndex..]);

            if (routeInfo.QueryStartIndex < 0 && path[^1] != '/')
                sbRoute.Append('/');

            sbRoute.Append(sbQueryParams);

            route = sbRoute.ToString();
        }

        return new Uri(route, UriKind.RelativeOrAbsolute);
    }

    /// <summary>
    /// Analiza la ruta para extraer información sobre los parámetros.
    /// </summary>
    /// <param name="path">Ruta a analizar.</param>
    /// <returns>RouteInfo con la información de los parámetros.</returns>
    private static RouteInfo GetRouteInfo(string path)
    {
        //bool hasRouteMatchParams = false;
        //bool hasQueryMatchParams = false;

        var matchParams = new List<MatchParam>();
        var sbParamName = new StringBuilder(path.Length);

        bool isSubtracting = false;

        const char startSubstractingChar = '{';
        const char endSubstractingChar = '}';
        const char startQueryParamChar = '?';

        int index = -1;
        int paramRouteIndex = 0;
        int queryStartIndex = -1;

        foreach (var c in path)
        {
            index++;
            
            if (!isSubtracting)
            {
                if (c.Equals(startSubstractingChar))
                {
                    isSubtracting = true;
                    paramRouteIndex = index;
                    sbParamName.Clear();
                }
                else if (c.Equals(startQueryParamChar))
                    queryStartIndex = index;

                continue;
            }

            if (c.Equals(endSubstractingChar))
            {
                isSubtracting = false;

                var match = new MatchParam(
                    sbParamName.ToString(),
                    queryStartIndex < 0 ? ParamType.Route : ParamType.Query,
                    paramRouteIndex
                );

                matchParams.Add(match);

                //hasRouteMatchParams = hasRouteMatchParams || queryStartIndex < 0;
                //hasQueryMatchParams = queryStartIndex >= 0;

                continue;
            }

            sbParamName.Append(c);
        }

        return new RouteInfo(
            //HasParams: hasRouteMatchParams || hasQueryMatchParams,
            //HasRouteMatchParams: hasRouteMatchParams,
            //HasQueryMatchParams: hasQueryMatchParams,
            QueryStartIndex: queryStartIndex,
            Params: matchParams.ToArray()
        );
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
    private readonly record struct RouteInfo(/*bool HasParams, bool HasRouteMatchParams, bool HasQueryMatchParams,*/ int QueryStartIndex, MatchParam[] Params);
}
