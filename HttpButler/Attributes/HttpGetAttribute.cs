namespace HttpButler.Attributes;

public class HttpGetAttribute : HttpMethodAttribute
{
    public override HttpMethod Method => HttpMethod.Get;

    public HttpGetAttribute()
    {
    }

    public HttpGetAttribute(string path) : base(path)
    {
    }
}
