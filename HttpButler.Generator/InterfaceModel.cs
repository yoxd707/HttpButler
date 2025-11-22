namespace HttpButler.Generator;

internal record struct InterfaceModel(string Namespace, string Name, List<MethodModel> Methods);

internal record struct MethodModel(
    string Name,
    string ReturnType,
    string ReturnTypeGenericArgument,
    bool IsGenericTask,
    string Route,
    HttpMethod HttpMethod,
    List<ParameterModel> Parameters
);

internal record struct ParameterModel(string Name, string Type, string? ExplicitDefaultValue, bool HasExplicitDefaultValue);
