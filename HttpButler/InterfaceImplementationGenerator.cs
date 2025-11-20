using System.Data;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace HttpButler;

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

        context.RegisterSourceOutput(combined, GenerateImplementation);
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

    private static void GenerateImplementation(SourceProductionContext ctx, ValueTuple<INamedTypeSymbol?, Compilation> tuple)
    {
        var symbol = tuple.Item1!;
        var compilation = tuple.Item2;

        var interfaceName = symbol.Name;
        var ns = symbol.ContainingNamespace.IsGlobalNamespace
            ? "Generated"
            : symbol.ContainingNamespace.ToString();

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
            var returnType = member.ReturnType.ToDisplayString();
            var methodName = member.Name;
            var parameters = member.Parameters
                .Select(p => !p.HasExplicitDefaultValue
                    ? p.ToDisplayString()
                    : $"{p.ToDisplayString()} = {p.ExplicitDefaultValue ?? "null"}"
                );

            var routeAttr = member.GetAttributes()
                .Where(attr =>
                    {
                        var attrClassName = attr.AttributeClass?.ToDisplayString();
                        return (attrClassName == "HttpButler.Attributes.HttpGetAttribute" ||
                            attrClassName == "HttpButler.Attributes.RouteAttribute") &&
                            attr.ConstructorArguments.Length > 0;
                    }
                )
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
                sb.AppendJoin(", ", parameters);

            sb.AppendLine(")");

            sb.Append(' ', 4)
                .AppendLine("{");

            // Ruta.
            sb.Append(' ', 8)
                .Append("const string route = \"")
                .Append(route)
                .AppendLine("\";");

            // Llamado al servicio.
            sb.Append(' ', 8)
                .Append("await _httpClientService.Get(\"")
                .Append(className)
                .Append("\", route, ");

            if (parameters.Any())
            {
                // Anónimo con los parametros.
                sb.AppendLine("new")
                    .Append(' ', 8)
                    .AppendLine("{")
                    .Append(' ', 12)
                    .AppendLine("test = 0")
                    .Append(' ', 8)
                    .AppendLine("}");
            }
            else
            {
                sb.Append("null");
            }

            sb.Append(' ', 8)
                .AppendLine(");");

            // Fin del método.
            sb.Append(' ', 4)
                .AppendLine("}");
        }

        sb.AppendLine("}");

        ctx.AddSource($"{className}.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }

}
