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
        IncrementalValuesProvider<InterfaceModel> interfaceInfos = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "HttpButler.Attributes.HttpButlerAttribute",
                predicate: static (node, _) => node is InterfaceDeclarationSyntax,
                transform: GetInterfaceModelOrNull
            )
            .Where(static m => m is not null)
            .Select(static (m, cancellationToken) => (InterfaceModel)m!);

        // Generate Implementations.
        context.RegisterSourceOutput(interfaceInfos, static (spc, model) =>
        {
            var source = GenerateImplementation(model);
            spc.AddSource($"gHttpButler_{model.Name}.g.cs", source);
        });

        // Service Collection.
        var collectedInterfaces = interfaceInfos.Collect();

        context.RegisterSourceOutput(collectedInterfaces, static (spc, models) =>
        {
            if (models.IsDefaultOrEmpty) return;

            var source = GenerateServiceCollectionExtension(models);
            spc.AddSource("gServiceCollectionExtension.g.cs", source);
        });
    }

    private static InterfaceModel? GetInterfaceModelOrNull(GeneratorAttributeSyntaxContext context, CancellationToken token)
    {
        var symbol = (INamedTypeSymbol)context.TargetSymbol;

        var methods = new List<MethodModel>();

        var taskSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
        var taskOfTSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");

        foreach (var member in symbol.GetMembers().OfType<IMethodSymbol>())
        {
            if (token.IsCancellationRequested) break;

            if (member.MethodKind != MethodKind.Ordinary) continue;

            // Lógica de extracción de atributos.
            var attributes = member.GetAttributes()
                .Where(x => x.AttributeClass?.ContainingNamespace?.ToDisplayString() == "HttpButler.Attributes");

            // TODO: Los métodos sin atributos deberían construirse arrojando una excepción.
            if (!attributes.Any()) continue;        // De momento ignoramos el método.

            var httpMethodEnum = GetHttpMethodFromAttributes(attributes);

            if (httpMethodEnum is null) continue;   // De momento ignoramos el método.

            var routeAttr = attributes
                .Where(attr => attr.ConstructorArguments.Length > 0)
                .FirstOrDefault();

            var route = routeAttr?.ConstructorArguments.FirstOrDefault().Value as string ?? "";

            // Analizar Retorno.
            bool isGenericTask = false;
            string returnTypeGenericArg = "";
            string returnTypeStr = member.ReturnType.ToDisplayString();

            if (SymbolEqualityComparer.Default.Equals(member.ReturnType.OriginalDefinition, taskOfTSymbol))
            {
                isGenericTask = true;
                if (member.ReturnType is INamedTypeSymbol namedRet && namedRet.TypeArguments.Length > 0)
                {
                    returnTypeGenericArg = namedRet.TypeArguments[0].ToDisplayString();
                }
            }
            else if (!SymbolEqualityComparer.Default.Equals(member.ReturnType, taskSymbol))
            {
                // No es Task ni Task<T>, de momento ignoramos.
                continue;
            }

            // Analizar Parámetros
            var parameters = new List<ParameterModel>();
            foreach (var p in member.Parameters)
            {
                var paramModel = new ParameterModel(
                    p.Name,
                    p.Type.ToDisplayString(),
                    p.HasExplicitDefaultValue ? p.ExplicitDefaultValue!.ToString() : null,
                    p.HasExplicitDefaultValue
                );

                parameters.Add(paramModel);
            }

            methods.Add(new MethodModel(
                member.Name,
                returnTypeStr,
                returnTypeGenericArg,
                isGenericTask,
                route,
                httpMethodEnum.Value,
                parameters
            ));
        }

        string ns = symbol.ContainingNamespace.IsGlobalNamespace ? "HttpButler.Generated" : symbol.ContainingNamespace.ToDisplayString();

        return new InterfaceModel(ns, symbol.Name, methods);
    }

    
    private static string GenerateImplementation(InterfaceModel interfaceModel)
    {
        var sb = new StringBuilder(1024);

        var className = $"gHttpButler_{interfaceModel.Name}";

        sb.AppendLine("#nullable enable")
            .AppendLine();

        // Namespace.
        sb.Append("namespace ")
            .Append(interfaceModel.Namespace)
            .AppendLine(";")
            .AppendLine();

        // Class.
        sb.Append("public class ")
            .Append(className)
            .Append(" : ")
            .AppendLine(interfaceModel.Name);

        sb.AppendLine("{")
            .AppendLine();

        // Fields.
        sb.Append(' ', 4)
            .AppendLine("private readonly HttpButler.Services.IHttpClientService _httpClientService;")
            .AppendLine();

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
            .AppendLine("}")
            .AppendLine();

        foreach (var method in interfaceModel.Methods)
        {
            // Inicio del método.
            sb.Append(' ', 4)
                .Append("public async ")
                .Append(method.ReturnType)
                .Append(' ')
                .Append(method.Name)
                .Append('(');

            foreach (var @param in method.Parameters)
            {
                if (sb[sb.Length - 1] != '(')
                    sb.Append(", ");

                sb.Append(@param.Type)
                    .Append(' ')
                    .Append(@param.Name);

                if (@param.HasExplicitDefaultValue)
                    sb.Append(" = ")
                        .Append(@param.ExplicitDefaultValue);
            }

            sb.AppendLine(")");

            sb.Append(' ', 4)
                .AppendLine("{");

            // Ruta.
            sb.Append(' ', 8)
                .Append("const string route = \"")
                .Append(method.Route)
                .AppendLine("\";");

            // Llamado al servicio.
            sb.Append(' ', 8);

            if (method.IsGenericTask)
            {
                sb.Append("return await _httpClientService.")
                    .Append(method.HttpMethod.ToString());

                if (method.ReturnType[method.ReturnType.Length - 1] == '?')
                    sb.Append("WithNullableResult<");
                else
                    sb.Append("<");

                sb.Append(method.ReturnTypeGenericArgument)
                    .Append(">(\"");
            }
            else
                sb.Append("await _httpClientService.")
                    .Append(method.HttpMethod.ToString())
                    .Append("(\"");

            sb.Append(className)
                .Append("\", route, ");

            // TODO: Separar en body, query, headers según los atributos de los parámetros.
            if (method.Parameters.Any())
            {
                // Anónimo con los parametros.
                sb.AppendLine("new")
                    .Append(' ', 8)
                    .AppendLine("{");

                foreach (var p in method.Parameters)
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
                .AppendLine("}")
                .AppendLine();
        }

        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string GenerateServiceCollectionExtension(ImmutableArray<InterfaceModel> models)
    {
        var sb = new StringBuilder(1024);

        sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        sb.AppendLine();
        sb.AppendLine("namespace HttpButler;");
        sb.AppendLine();
        sb.AppendLine("public static class gServiceCollectionExtension");
        sb.AppendLine("{");
        sb.AppendLine("    public static IServiceCollection AddHttpButler(this IServiceCollection services)");
        sb.AppendLine("    {");

        foreach (var item in models)
        {
            sb.AppendLine(
                $"        services.AddHttpButler<{item.Namespace}.{item.Name}, {item.Namespace}.gHttpButler_{item.Name}>();"
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
