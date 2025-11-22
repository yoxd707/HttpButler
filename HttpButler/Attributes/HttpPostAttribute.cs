namespace HttpButler.Attributes;

public class HttpPostAttribute : HttpMethodAttribute
{
    public override HttpMethod Method => HttpMethod.Post;

    public HttpPostAttribute()
    {
    }

    public HttpPostAttribute(string path) : base(path)
    {
    }
}
