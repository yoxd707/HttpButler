namespace HttpButler.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface)]
public class RouteAttribute : Attribute
{
    public string Path { get; set; }

    public RouteAttribute(string path)
    {
        Path = path;
    }
}
