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

        sb.Append("namespace ")
            .Append(ns)
            .AppendLine(";");

        sb.Append("public class gHttpButler_")
            .Append(interfaceName)
            .Append(" : ")
            .AppendLine(interfaceName);

        sb.AppendLine("{");

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

            sb.Append(' ', 4)
                .Append("public ")
                .Append(returnType)
                .Append(' ')
                .Append(methodName)
                .Append('(');

            if (parameters.Any())
                sb.AppendJoin(", ", parameters);

            sb.AppendLine(")");

            sb.Append(' ', 4)
                .AppendLine("{");



            sb.Append(' ', 4)
                .AppendLine("}");
        }

        sb.AppendLine("}");

        ctx.AddSource($"gHttpButler_{interfaceName}.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));

        //var compilation = context.Compilation;

        //var httpButlerSymbol = compilation.GetTypeByMetadataName("HttpButler.Attributes.HttpButlerAttribute");
        //if (httpButlerSymbol is null) return;

        //var root = compilation.SyntaxTrees.First().GetRoot();

        //foreach (var tree in compilation.SyntaxTrees)
        //{
        //    var model = compilation.GetSemanticModel(tree);

        //    var interfaces = root
        //        .DescendantNodes()
        //        .OfType<InterfaceDeclarationSyntax>();

        //    foreach (var interfaceSyntax in interfaces)
        //    {
        //        if (model.GetDeclaredSymbol(interfaceSyntax) is not INamedTypeSymbol interfaceSymbol)
        //            continue;

        //        var hasHttpButlerAttribute = interfaceSymbol
        //            .GetAttributes()
        //            .Any(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, httpButlerSymbol));

        //        if (!hasHttpButlerAttribute)
        //            continue;

        //        GenerateImplementation(context, interfaceSymbol);
        //    }
        //}
    }

}
