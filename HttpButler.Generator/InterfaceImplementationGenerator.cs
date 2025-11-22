using System.Text;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;

namespace HttpButler.Generator;

[Generator]
public class InterfaceImplementationGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var interfaces = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: IsInterfaceWithAttributes,
                transform: GetInterfaceSymbol
            )
            .Where(static symbol => symbol is not null)!;

        var compilation = context.CompilationProvider;

        var combined = interfaces.Combine(compilation);

        var implementations = combined.Select(
            static (tuple, _) =>
            {
                var (iface, compilation) = tuple;

                var (ns, implName, implSource) = GenerateImplementation(compilation, iface!);

                return (iface, ns, implName, implSource);
            }
        )
        .Where(x => x.implSource is not null)!;

        context.RegisterSourceOutput(
            implementations,
            static (spc, item) =>
            {
                spc.AddSource($"{item.implName}.g.cs", item.implSource!);
            }
        );

        context.RegisterSourceOutput(
            implementations.Collect(),
            static (spc, list) =>
            {
                var source = GenerateServiceCollectionExtension(list);
                spc.AddSource("gServiceCollectionExtension.g.cs", source);
            }
        );
    }

    private static bool IsInterfaceWithAttributes(SyntaxNode node, CancellationToken token)
        => node is InterfaceDeclarationSyntax interfaceDecl &&
           interfaceDecl.AttributeLists.Any();

    private static INamedTypeSymbol? GetInterfaceSymbol(
        GeneratorSyntaxContext ctx,
        CancellationToken token)
    {
        if (ctx.SemanticModel
            .GetDeclaredSymbol((InterfaceDeclarationSyntax)ctx.Node, token) is not INamedTypeSymbol symbol)
            return null;

        if (symbol.GetAttributes().Any(attr => attr.AttributeClass?.ToDisplayString() == "HttpButler.Attributes.HttpButlerAttribute"))
            return symbol;

        return null;
    }

    private static (string ns, string implName, string implSource) GenerateImplementation(Compilation compilation, INamedTypeSymbol symbol)
    {
        var interfaceName = symbol.Name;

        string? ns = null;

        if (!symbol.ContainingNamespace.IsGlobalNamespace)
            ns = symbol.ContainingNamespace.ToString();

        ns ??= "HttpButler.Generated";

        var sb = new StringBuilder();

        var className = $"gHttpButler_{interfaceName}";

        // Namespace.
        sb.Append("namespace ")
            .Append(ns)
            .AppendLine(";");

        // Class.
        sb.Append("public class ")
            .Append(className)
            .Append(" : ")
            .AppendLine(interfaceName);

        sb.AppendLine("{");

        // Fields.
        sb.Append(' ', 4)
            .AppendLine("private readonly HttpButler.Services.IHttpClientService _httpClientService;");

        // Constructor.
        sb.Append(' ', 4)
            .Append("public ")
            .Append(className)
            .AppendLine("(HttpButler.Services.IHttpClientService httpClientService)");

        sb.Append(' ', 4)
            .AppendLine("{");

        sb.Append(' ', 8)
            .AppendLine("_httpClientService = httpClientService;");

        sb.Append(' ', 4)
            .AppendLine("}");

        var taskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
        var taskOfTypeSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");

        var methods = symbol
            .GetMembers()
            .OfType<IMethodSymbol>()
            .Where(m => m.MethodKind == MethodKind.Ordinary)
            .Where(m =>
                SymbolEqualityComparer.Default.Equals(m.ReturnType, taskSymbol) ||
                (
                    m.ReturnType is INamedTypeSymbol namedType &&
                    SymbolEqualityComparer.Default.Equals(namedType.OriginalDefinition, taskOfTypeSymbol)
                )
            );

        foreach (var member in methods)
        {
            var attributes = member.GetAttributes()
                .Where(attr =>
                {
                    var attrClassName = attr.AttributeClass?.Name;
                    return attrClassName == "HttpGetAttribute" ||
                        attrClassName == "HttpPostAttribute" ||
                        attrClassName == "RouteAttribute";
                });

            // TODO: Los métodos sin atributos deberían construirse arrojando una excepción.
            if (!attributes.Any()) continue;    // De momento ignoramos el método.

            var httpMethod = GetHttpMethodFromAttributes(attributes);

            if (httpMethod is null) continue;   // De momento ignoramos el método.

            var returnType = member.ReturnType.ToDisplayString();
            var methodName = member.Name;
            var parameters = member.Parameters;

            var taskResultTypeIndex = returnType.IndexOf('<');
            var taskResultType = string.Empty;

            if (taskResultTypeIndex > 0)
                taskResultType = returnType.Substring(taskResultTypeIndex + 1, returnType.Length - (taskResultTypeIndex + 2));

            var routeAttr = attributes
                .Where(attr => attr.ConstructorArguments.Length > 0)
                .FirstOrDefault();

            var route = (routeAttr?.ConstructorArguments.FirstOrDefault().Value as string) ?? string.Empty;

            // Inicio del método.
            sb.Append(' ', 4)
                .Append("public async ")
                .Append(returnType)
                .Append(' ')
                .Append(methodName)
                .Append('(');

            if (parameters.Any())
            {
                var @params = string.Join(", ", parameters.Select(
                    p => !p.HasExplicitDefaultValue
                        ? p.ToDisplayString()
                        : $"{p.ToDisplayString()} = {p.ExplicitDefaultValue ?? "null"}"
                ));

                sb.Append(@params);
            }

            sb.AppendLine(")");

            sb.Append(' ', 4)
                .AppendLine("{");

            // Ruta.
            sb.Append(' ', 8)
                .Append("const string route = \"")
                .Append(route)
                .AppendLine("\";");

            // Llamado al servicio.
            sb.Append(' ', 8);

            if (taskResultTypeIndex > 0)
            {
                sb.Append("return await _httpClientService.")
                    .Append(httpMethod.ToString());

                if (taskResultType[taskResultType.Length - 1] == '?')
                    sb.Append("WithNullableResult<");
                else
                    sb.Append("<");

                sb.Append(taskResultType)
                    .Append(">(\"");
            }
            else
                sb.Append("await _httpClientService.")
                    .Append(httpMethod.ToString())
                    .Append("(\"");

            sb.Append(className)
                .Append("\", route, ");

            // TODO: Separar en body, query, headers según los atributos de los parámetros.
            if (parameters.Any())
            {
                // Anónimo con los parametros.
                sb.AppendLine("new")
                    .Append(' ', 8)
                    .AppendLine("{");

                foreach (var p in parameters)
                    sb.Append(' ', 12)
                        .Append(p.Name)
                        .AppendLine(",");

                sb.Append(' ', 8)
                    .AppendLine("});");
            }
            else
            {
                sb.AppendLine("null);");
            }

            // Fin del método.
            sb.Append(' ', 4)
                .AppendLine("}");
        }

        sb.AppendLine("}");

        return (ns, className, sb.ToString());
    }

    private static string GenerateServiceCollectionExtension(ImmutableArray<(INamedTypeSymbol? iface, string ns, string implName, string implSource)> list)
    {
        var sb = new StringBuilder();

        sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        sb.AppendLine($"namespace HttpButler;");
        sb.AppendLine();
        sb.AppendLine("public static class gServiceCollectionExtension");
        sb.AppendLine("{");
        sb.AppendLine("    public static IServiceCollection AddHttpButler(this IServiceCollection services)");
        sb.AppendLine("    {");

        foreach (var item in list)
        {
            sb.AppendLine(
                $"        services.AddHttpButler<{item.iface!.ToDisplayString()}, {item.ns}.{item.implName}>();"
            );
        }

        sb.AppendLine("        return services;");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }


    private static HttpMethod? GetHttpMethodFromAttributes(IEnumerable<AttributeData> attributes)
    {
        var httpMethodAttr = attributes.FirstOrDefault(attr => attr.AttributeClass!.Name[0] == 'H');

        if (httpMethodAttr is null || httpMethodAttr.AttributeClass is null)
            return null;

        var httpMethodAttrName = httpMethodAttr.AttributeClass.Name;
        var httpMethodStr = httpMethodAttrName.Substring(4, httpMethodAttrName.Length - 13);

        if (!Enum.TryParse<HttpMethod>(httpMethodStr, out var httpMethod))
            return null;

        return httpMethod;
    }

}
