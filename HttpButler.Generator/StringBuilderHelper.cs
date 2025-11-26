using System.Text;

namespace HttpButler.Generator;

internal static class StringBuilderHelper
{
    public static StringBuilder AppendNullableContext(this StringBuilder sb)
        => sb.AppendLine("#nullable enable");

    public static StringBuilder AppendNamespace(this StringBuilder sb, InterfaceModel iface)
        => sb.Append("namespace ")
            .Append(iface.Namespace)
            .AppendLine(";");

    public static StringBuilder AppendClassDeclaration(this StringBuilder sb, InterfaceModel iface)
        => sb.Append("public class ")
            .Append(iface.ClassName)
            .Append(" : ")
            .AppendLine(iface.Name);

    public static StringBuilder AppendFieldDeclaration(this StringBuilder sb, InterfaceModel iface)
    {
        foreach (var field in iface.Fields)
            sb.AppendIdentation()
                .Append("private readonly ")
                .Append(field.Type)
                .Append(' ')
                .Append(field.Name)
                .AppendLine(";");

        return sb;
    }

    public static StringBuilder AppendClassConstructor(this StringBuilder sb, InterfaceModel iface)
    {
        sb.AppendIdentation()
            .Append("public ")
            .Append(iface.ClassName)
            .Append('(')
            .AppendParameters(iface.ClassConstructor.Parameters)
            .AppendLine(")")
            .AppendIdentation()
            .AppendLine("{");

        foreach (var (param, field) in iface.ClassConstructor.ParamFieldMappings)
            sb.AppendIdentation(2)
                .Append("this.")
                .Append(field.Name)
                .Append(" = ")
                .Append(param.Name)
                .AppendLine(";");

        sb.AppendIdentation()
            .AppendLine("}");

        return sb;
    }

    public static StringBuilder AppendMethod(this StringBuilder sb, MethodModel method)
        => sb.AppendIdentation()
            .Append("public async ")
            .Append(method.ReturnType)
            .Append(' ')
            .Append(method.Name)
            .Append('(')
            .AppendParameters(method.Parameters)
            .AppendLine(")");

    public static StringBuilder AppendParameters(this StringBuilder sb, IEnumerable<ParameterModel> parameters)
    {
        var firstParam = 0;
        foreach (var param in parameters)
        {
            sb.Append(firstParam++ == 0 ? string.Empty : ", ")
                .Append(param.Type)
                .Append(' ')
                .Append(param.Name);

            if (!param.HasExplicitDefaultValue) continue;

            sb.Append(" = ")
                .Append(param.ExplicitDefaultValue);
        }

        return sb;
    }

    public static StringBuilder AppendIdentation(this StringBuilder sb, int identLevel = 1)
        => sb.Append(' ', identLevel * 4);
}
