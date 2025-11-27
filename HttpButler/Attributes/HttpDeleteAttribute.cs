namespace HttpButler.Attributes;

public class HttpDeleteAttribute : HttpMethodAttribute
{
    public override HttpMethod Method => HttpMethod.Delete;

    public HttpDeleteAttribute()
    {
    }

    public HttpDeleteAttribute(string path) : base(path)
    {
    }
}
