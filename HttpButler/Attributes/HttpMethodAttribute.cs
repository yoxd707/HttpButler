namespace HttpButler.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public abstract class HttpMethodAttribute : Attribute
{
    public abstract HttpMethod Method { get; }
    public virtual string? Path { get; protected set; }

    protected HttpMethodAttribute()
    {
    }

    protected HttpMethodAttribute(string path)
    {
        Path = path;
    }
}
