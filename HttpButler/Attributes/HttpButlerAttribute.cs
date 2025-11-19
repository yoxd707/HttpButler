namespace HttpButler.Attributes;

[AttributeUsage(AttributeTargets.Interface)]
public sealed class HttpButlerAttribute : Attribute
{
    public Type[] Injections { get; }

    public HttpButlerAttribute() : this(Array.Empty<Type>())
    {
        
    }

    public HttpButlerAttribute(params Type[] types)
    {
        Injections = types;
    }
}
