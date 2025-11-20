using HttpButler.Utils;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;

namespace HttpButler.Services;

public class PathResolveService : IPathResolveService
{
    private record RouteParam(string ParamName, int StartIndex);

    private static readonly ConcurrentDictionary<Guid, RouteParam[]> cache = new();

    public Uri ResolveUri(string path, object? parameters = null)
    {
        var route = path;

        if (parameters is not null)
        {
            var cacheKey = DeterministicGuid.FromString(route);
            var routeParams = cache.GetOrAdd(cacheKey, key => GetRouteParameters(route));

            if (routeParams.Length > 0)
            {
                var sb = new StringBuilder();
                var properties = parameters.GetType().GetProperties();

                var lastStart = 0;
                foreach (var p in routeParams)
                {
                    var start = lastStart;
                    var end = p.StartIndex;
                    var prop = properties.First(x => EqualsName(x, p.ParamName));
                    var propValue = Uri.EscapeDataString(prop.GetValue(parameters)!.ToString()!);

                    sb.Append(path[start..end])
                        .Append(propValue);

                    lastStart = end + p.ParamName.Length + 2;
                }

                if (path.Length > lastStart)
                    sb.Append(path[lastStart..]);

                route = sb.ToString();
            }
        }

        return new Uri(route, UriKind.RelativeOrAbsolute);
    }

    private static bool EqualsName(PropertyInfo property, string name)
        => property.Name.Equals(name, StringComparison.OrdinalIgnoreCase);

    private static RouteParam[] GetRouteParameters(string path)
    {
        var routeParams = new List<RouteParam>();
        var sb = new StringBuilder(path.Length);

        bool isSubtracting = false;

        const char startSubstractingChar = '{';
        const char endSubstractingChar = '}';

        int index = -1;
        int routeParamStartIndex = 0;
        foreach (var c in path)
        {
            index++;
            
            if (!isSubtracting)
            {
                if (c.Equals(startSubstractingChar))
                {
                    isSubtracting = true;
                    routeParamStartIndex = index;
                    sb.Clear();
                }

                continue;
            }

            if (c.Equals(endSubstractingChar))
            {
                isSubtracting = false;
                routeParams.Add(new RouteParam(sb.ToString(), routeParamStartIndex));
                continue;
            }

            sb.Append(c);
        }

        return routeParams.ToArray();
    }

}
