using HttpButler.Attributes;
using HttpButler.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Text.Json;

namespace HttpButler;

/// <summary>
/// Proveedor de opciones para configurar los servicios.
/// </summary>
public class HttpButlerOptions
{
    private readonly Dictionary<Type, Type> _interfaces;
    private readonly Dictionary<string, JsonSerializerOptions> _jsonOptions;
    private JsonSerializerOptions _defaultJsonOptions;

    public HttpButlerOptions()
    {
        _interfaces = [];
        _jsonOptions = [];
        _defaultJsonOptions = new();
    }

    /// <summary>
    /// Registra todas las interfaces marcadas con el atributo <see cref="HttpButlerAttribute"/>
    /// </summary>
    /// <param name="assembly">Assembly del que se tomarán todas las interfaces.</param>
    public HttpButlerOptions RegisterHttpInterfaces(Assembly? assembly = null)
    {
        assembly ??= Assembly.GetExecutingAssembly();

        var interfaces = assembly.GetTypes()
            .Where(t => t.IsInterface && t.GetCustomAttributes(typeof(HttpButlerAttribute), true).Any());

        foreach (var iface in interfaces)
        {
            var impl = assembly.GetType($"{iface.Namespace}.gHttpButler_{iface.Name}");
            AddHttpInterface(iface, impl);
        }

        return this;
    }

    /// <summary>
    /// Agrega una interface.
    /// </summary>
    /// <typeparam name="TIface">Interface.</typeparam>
    /// <param name="jsonOptions">Opciones para el serializador de JSON.</param>
    public HttpButlerOptions AddHttpInterface<TIface>(JsonSerializerOptions ? jsonOptions = null)
        where TIface : class
        => AddHttpInterface(typeof(TIface), impl: null, jsonOptions);

    /// <summary>
    /// Agrega una interface.
    /// </summary>
    /// <typeparam name="TIface">Interface.</typeparam>
    /// <typeparam name="TImpl">Implementación de la interface.</typeparam>
    /// <param name="jsonOptions">Opciones para el serializador de JSON.</param>
    public HttpButlerOptions AddHttpInterface<TIface, TImpl>(JsonSerializerOptions? jsonOptions = null)
        where TIface : class
        where TImpl : class, TIface
        => AddHttpInterface(typeof(TIface), typeof(TImpl), jsonOptions);

    /// <summary>
    /// Agrega una interface.
    /// </summary>
    /// <param name="iface">Interface.</param>
    /// <param name="impl">Implementación de la interface.</param>
    /// <param name="jsonOptions">Opciones para el serializador de JSON.</param>
    internal HttpButlerOptions AddHttpInterface(Type iface, Type? impl, JsonSerializerOptions? jsonOptions = null)
    {
        if (!_interfaces.ContainsKey(iface))
        {
            impl ??= iface.Assembly.GetType($"{iface.Namespace}.gHttpButler_{iface.Name}");
            _interfaces.Add(iface, impl);
        }

        if (jsonOptions is not null)
            AddInterfaceJsonOptions(iface, jsonOptions);

        return this;
    }

    /// <summary>
    /// Establece el <see cref="JsonSerializerOptions"/> a usar por defecto.
    /// En caso de no establecer las opciones, se usarán las opciones predeterminadas del <see cref="JsonSerializer"/>.
    /// </summary>
    /// <param name="jsonOptions">Opciones para el serializador de JSON.</param>
    public HttpButlerOptions AddDefaultJsonOptions(JsonSerializerOptions jsonOptions)
    {
        _defaultJsonOptions = jsonOptions;
        return this;
    }

    /// <summary>
    /// Establece el <see cref="JsonSerializerOptions"/> a usar para la interface específicada.
    /// Esto sobrescribe las opciones usadas por defecto.
    /// </summary>
    /// <typeparam name="TIface">Interface a configurar.</typeparam>
    /// <param name="jsonOptions">Opciones para el serializador de JSON.</param>
    public HttpButlerOptions AddInterfaceJsonOptions<TIface>(JsonSerializerOptions jsonOptions)
        => AddInterfaceJsonOptions(typeof(TIface), jsonOptions);

    /// <summary>
    /// Establece el <see cref="JsonSerializerOptions"/> a usar para la interface específicada.
    /// Esto sobrescribe las opciones usadas por defecto.
    /// </summary>
    /// <param name="iface">Interface a configurar.</param>
    /// <param name="jsonOptions">Opciones para el serializador de JSON.</param>
    internal HttpButlerOptions AddInterfaceJsonOptions(Type iface, JsonSerializerOptions jsonOptions)
    {
        var implTypeName = $"gHttpButler_{iface.Name}";
        _jsonOptions.Add(implTypeName, jsonOptions);
        return this;
    }

    /// <summary>
    /// Registra las opciones configuradas y servicios necesarios.
    /// </summary>
    /// <param name="services">Colección de servicos.</param>
    internal void Register(IServiceCollection services)
    {
        bool constainsService(Type serviceType)
            => services.Where(x => x.ServiceType == serviceType).Any();

        if (!constainsService(typeof(IJsonOptionsResolver)))
            services.AddSingleton<IJsonOptionsResolver>(new JsonOptionsResolver(_defaultJsonOptions, _jsonOptions));

        if (!constainsService(typeof(IPathResolveService)))
            services.AddScoped<IPathResolveService, PathResolveService>();

        if (!constainsService(typeof(IHttpClientService)))
            services.AddScoped<IHttpClientService, HttpClientService>();

        foreach (var item in _interfaces)
        {
            var iface = item.Key;
            var impl = item.Value;

            if (constainsService(iface))
                continue;
            
            services.AddScoped(iface, impl);

            // TODO: Separar la lógica, para permitir configuración
            // personalizada de IHttpClientFactory por interfaz y un Default global.
            // https://github.com/yoxd707/HttpButler/issues/2
            Action<HttpClient> configureClient = opt => { };

            var attr = iface.GetCustomAttributes(typeof(RouteAttribute), true);

            if (attr.Any())
            {
                var route = (RouteAttribute)attr.First();
                var baseAdress = new Uri(route.Path);
                configureClient = opt =>
                {
                    opt.BaseAddress = baseAdress;
                };
            }

            services.AddHttpClient(impl.Name, configureClient);
        }
    }
}
