using Microsoft.Extensions.DependencyInjection;

namespace HttpButler;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddHttpButler(this IServiceCollection services, Type interfaceType)
    {
        return services;
    }
}
