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
            var classBuilder = new ClassBuilder(model);
            var source = classBuilder.BuildClass();
            spc.AddSource($"{model.ClassName}.g.cs", source);
        });

        // Service Collection.
        var collectedInterfaces = interfaceInfos.Collect();

        context.RegisterSourceOutput(collectedInterfaces, static (spc, models) =>
        {
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
            if (token.IsCancellationRequested) return null;

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
                    p.GetAttributes()
                        .Where(a => a.AttributeClass is not null)
                        .Select(a => a.AttributeClass!.Name),
                    p.HasExplicitDefaultValue,
                    p.HasExplicitDefaultValue ? p.ExplicitDefaultValue!.ToString() : null
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
        string className = $"gHttpButler_{symbol.Name}";

        var constructorParameters = new List<ParameterModel>
        {
            new(
                "httpClientService",
                "HttpButler.Services.IHttpClientService",
                Attributes: []
            )
        };

        var fields = new List<FieldModel>
        {
            new(
                "_httpClientService",
                "HttpButler.Services.IHttpClientService"
            )
        };

        var paramFieldMappings = constructorParameters.Join<ParameterModel, FieldModel, string, (ParameterModel param, FieldModel field)>
            (fields, x => x.Type, x => x.Type, static (param, field) => new (param, field))
            .ToList();

        var constructorModel = new ClassConstructorModel(
            constructorParameters,
            paramFieldMappings
        );

        return new InterfaceModel(ns, symbol.Name, className, methods, fields, constructorModel);
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

        //if (models.Length > 0)
        //{
        //    sb.AppendLine($"        var assembly = System.Reflection.Assembly.GetAssembly(typeof({models[0].Namespace}.{models[0].Name}));");
        //    sb.AppendLine($"        ServiceCollectionExtension.AddHttpButler(services, assembly);");
        //}

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
