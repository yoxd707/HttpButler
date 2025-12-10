using System.Text.Json;

namespace HttpButler.Services;

public interface IJsonOptionsResolver
{
    JsonSerializerOptions GetJsonOptions(string key);
}
