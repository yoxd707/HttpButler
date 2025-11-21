using HttpButler.Attributes;
using HttpButler.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HttpButler;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddHttpButler<T>(this IServiceCollection services)
        where T : class
        => AddHttpButler(services, typeof(T));

    public static IServiceCollection AddHttpButler<T>(this IServiceCollection services, Action<HttpClient> configureClient)
        where T : class
        => AddHttpButler(services, typeof(T), configureClient);

    public static IServiceCollection AddHttpButler(this IServiceCollection services, Type interfaceType)
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

        return AddHttpButler(services, interfaceType, configureClient);
    }

    public static IServiceCollection AddHttpButler(this IServiceCollection services, Type interfaceType, Action<HttpClient> configureClient)
    {
        services.AddHttpClient($"gHttpButler_{interfaceType.Name}", configureClient);
        services.AddScoped<IPathResolveService, PathResolveService>();
        services.AddScoped<IHttpClientService, HttpClientService>();
        return services;
    }
}
