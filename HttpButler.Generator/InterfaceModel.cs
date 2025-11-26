namespace HttpButler.Generator;

internal record struct InterfaceModel
(
    string Namespace,
    string Name,
    string ClassName,
    List<MethodModel> Methods,
    List<FieldModel> Fields,
    ClassConstructorModel ClassConstructor
);

internal record struct MethodModel
(
    string Name,
    string ReturnType,
    string ReturnTypeGenericArgument,
    bool IsGenericTask,
    string Route,
    HttpMethod HttpMethod,
    List<ParameterModel> Parameters
);

internal record struct ClassConstructorModel
(
    List<ParameterModel> Parameters,
    List<(ParameterModel param, FieldModel field)> ParamFieldMappings
);

internal record struct ParameterModel
(
    string Name,
    string Type,
    IEnumerable<string> Attributes,
    bool HasExplicitDefaultValue = false,
    string? ExplicitDefaultValue = null
);

internal record struct FieldModel
(
    string Name,
    string Type
);
