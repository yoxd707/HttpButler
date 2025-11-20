namespace HttpButler.Services;

public interface IPathResolveService
{
    Uri ResolveUri(string path, object? parameters = null);
}
