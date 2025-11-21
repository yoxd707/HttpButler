namespace HttpButler;

public sealed class HttpRouteResolveException : Exception
{
    private const string DefaultMessage = "Error resolving HTTP route with the provided parameters.";

    public HttpRouteResolveException() : base(DefaultMessage)
    {
    }

    public HttpRouteResolveException(string? message) : base(message)
    {
    }
}
