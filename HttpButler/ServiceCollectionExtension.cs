using Microsoft.Extensions.DependencyInjection;

namespace HttpButler;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddHttpButler(this IServiceCollection services, Action<HttpButlerOptions> configure)
    {
        var options = new HttpButlerOptions();
        configure(options);
        options.Register(services);
        return services;
    }
}
