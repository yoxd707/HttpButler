using System.Text;

namespace HttpButler.Generator;

internal class ClassBuilder(InterfaceModel ifaceModel)
{
    private int _identLevel = 0;
    private readonly StringBuilder _stringBuilder = new(1024);
    private readonly InterfaceModel _ifaceModel = ifaceModel;

    public string BuildClass()
    {
        _stringBuilder.AppendNullableContext()
            .AppendLine()
            .AppendNamespace(_ifaceModel)
            .AppendLine();

        // Class.
        _stringBuilder.AppendClassDeclaration(_ifaceModel);

        AppendOpenKey();
        _stringBuilder.AppendLine();

        // Fields.
        _stringBuilder.AppendFieldDeclaration(_ifaceModel)
            .AppendLine();

        // Constructor.
        _stringBuilder.AppendClassConstructor(_ifaceModel)
            .AppendLine();

        foreach (var method in _ifaceModel.Methods)
            BuildMethod(method);

        AppendClosedKey();

        return _stringBuilder.ToString();
    }

    private void AppendOpenKey(bool appendLine = true)
    {
        if (appendLine)
            _stringBuilder.AppendLine("{");
        else
            _stringBuilder.Append('{');

        _identLevel++;
    }

    private void AppendClosedKey(bool appendLine = true)
    {
        _identLevel--;
        _stringBuilder.AppendIdentation(_identLevel);

        if (appendLine)
            _stringBuilder.AppendLine("}");
        else
            _stringBuilder.Append('}');
    }

    private void AppendAnonymous(params IEnumerable<string> fields)
    {
        _stringBuilder.AppendLine("new")
            .AppendIdentation(_identLevel);

        AppendOpenKey();

        foreach (var p in fields)
            _stringBuilder.AppendIdentation(_identLevel)
                .Append(p)
                .AppendLine(",");

        AppendClosedKey(appendLine: false);
    }

    private void BuildMethod(MethodModel method)
    {
        // Inicio del método.
        _stringBuilder.AppendMethod(method)
            .AppendIdentation(_identLevel);

        AppendOpenKey();

        if (method.ReturnType.IsGeneric &&
            !method.ReturnType.GenericArguments[0].IsNullable &&
            method.ReturnType.GenericArguments[0].IsReferenceType)
            _stringBuilder.AppendIdentation(_identLevel)
                .Append("#warning \"The method ")
                .Append(method.Name)
                .Append(" of ")
                .Append(_ifaceModel.Name)
                .AppendLine(" contains a not nullable reference type, but if the http request fails, it returns a default type (null).\".");

        // Ruta.
        _stringBuilder.AppendIdentation(_identLevel)
                .Append("const string route = \"")
                .Append(method.Route)
                .AppendLine("\";");

        // Query and Route Params.
        var queryParams = method.Parameters
            .Where(p => !p.Attributes.Any() || p.Attributes.Where(a => a == "ToQueryAttribute" || a == "ToRouteAttribute").Any());

        var hasQueryParams = queryParams.Any();

        if (hasQueryParams)
        {
            _stringBuilder.AppendIdentation(_identLevel)
                .Append("var qParams = ");

            AppendAnonymous(queryParams.Select(p => p.Name));

            _stringBuilder.AppendLine(";");
        }

        // Body Params.
        IEnumerable<ParameterModel> bodyParams;
        var acceptBodyParams = method.HttpMethod == HttpMethod.Post || method.HttpMethod == HttpMethod.Put;
        var hasBodyParams = false;

        if (acceptBodyParams)
        {
            bodyParams = method.Parameters
                .Where(p => p.Attributes.Where(a => a == "ToBodyAttribute").Any());

            hasBodyParams = bodyParams.Any();

            if (hasBodyParams)
            {
                _stringBuilder.AppendIdentation(_identLevel)
                    .Append("object bParams = ");

                if (bodyParams.Count() == 1)
                    _stringBuilder.Append(bodyParams.First().Name);
                else
                    AppendAnonymous(bodyParams.Select(p => p.Name));

                _stringBuilder.AppendLine(";");
            }
        }

        // Llamado al servicio.
        _stringBuilder.AppendIdentation(_identLevel);

        if (method.ReturnType.IsGeneric)
        {
            _stringBuilder.Append("return await _httpClientService.")
                .Append(method.HttpMethod.ToString());

            if (method.ReturnType.GenericArguments[0].IsNullable)
                _stringBuilder.Append("WithNullableResult<");
            else
                _stringBuilder.Append("<");

            _stringBuilder.Append(method.ReturnType.GenericArguments[0].StringRepresentation)
                .Append(">(\"");
        }
        else
            _stringBuilder.Append("await _httpClientService.")
                .Append(method.HttpMethod.ToString())
                .Append("(\"");

        _stringBuilder.Append(_ifaceModel.ClassName)
            .Append("\", route, ");

        _stringBuilder.Append(hasQueryParams ? "qParams" : "null");

        if (acceptBodyParams)
            _stringBuilder.Append(", ")
                .Append(hasBodyParams ? "bParams" : "null");

        _stringBuilder.AppendLine(");");

        // Fin del método.
        AppendClosedKey();
        _stringBuilder.AppendLine();
    }

}
