using HttpButler.Attributes;
using HttpButler.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace HttpButler;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddHttpButler<TIface, TImpl>(this IServiceCollection services)
        where TIface : class
        where TImpl : class, TIface
    {
        services.AddScoped<TIface, TImpl>();
        AddHttpClient(services, typeof(TIface));

        // Solo se agregan una vez los servicios auxiliares.
        if (!services.Any(x => x.ServiceType == typeof(IPathResolveService)))
        {
            services.AddScoped<IPathResolveService, PathResolveService>();
            services.AddScoped<IHttpClientService, HttpClientService>();
        }

        return services;
    }

    public static IServiceCollection AddHttpButler(this IServiceCollection services, Assembly? assembly)
    {
        if (assembly is null)
            throw new ArgumentNullException(nameof(assembly));

        var interfaces = assembly.GetTypes()
            .Where(t => t.IsInterface && t.GetCustomAttributes(typeof(HttpButlerAttribute), true).Any());

        foreach (var iface in interfaces)
        {
            var impl = assembly.GetType($"{iface.Namespace}.gHttpButler_{iface.Name}");
            
            if (impl != null)
            {
                services.AddScoped(iface, impl);
                AddHttpClient(services, iface);
            }
        }

        services.AddScoped<IPathResolveService, PathResolveService>();
        services.AddScoped<IHttpClientService, HttpClientService>();

        return services;
    }

    internal static IServiceCollection AddHttpClient(this IServiceCollection services, Type interfaceType)
    {
        Action<HttpClient> configureClient = opt => { };

        var attr = interfaceType.GetCustomAttributes(typeof(RouteAttribute), true);

        if (attr.Any())
        {
            var route = (RouteAttribute)attr.First();
            var baseAdress = new Uri(route.Path);
            configureClient = opt =>
            {
                opt.BaseAddress = baseAdress;
            };
        }

        return AddHttpClient(services, interfaceType, configureClient);
    }

    internal static IServiceCollection AddHttpClient(this IServiceCollection services, Type interfaceType, Action<HttpClient> configureClient)
    {
        var implTypeName = $"gHttpButler_{interfaceType.Name}";
        services.AddHttpClient(implTypeName, configureClient);
        return services;
    }
}
