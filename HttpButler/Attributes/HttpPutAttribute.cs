namespace HttpButler.Attributes;

public class HttpPutAttribute : HttpMethodAttribute
{
    public override HttpMethod Method => HttpMethod.Put;

    public HttpPutAttribute()
    {
    }

    public HttpPutAttribute(string path) : base(path)
    {
    }
}
