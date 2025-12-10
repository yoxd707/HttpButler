using System.Collections.ObjectModel;
using System.Text.Json;

namespace HttpButler.Services;

public class JsonOptionsResolver : IJsonOptionsResolver
{
    private readonly JsonSerializerOptions _defaultJsonOptions;
    private readonly IReadOnlyDictionary<string, JsonSerializerOptions> _jsonOptionsDictionary;

    public JsonOptionsResolver()
    {
        _defaultJsonOptions = JsonSerializerOptions.Default;
        _jsonOptionsDictionary = new ReadOnlyDictionary<string, JsonSerializerOptions>(new Dictionary<string, JsonSerializerOptions>());
    }

    public JsonOptionsResolver(JsonSerializerOptions defaultJsonOptions)
        : this(defaultJsonOptions, new ReadOnlyDictionary<string, JsonSerializerOptions>(new Dictionary<string, JsonSerializerOptions>()))
    {
    }

    public JsonOptionsResolver(IReadOnlyDictionary<string, JsonSerializerOptions> jsonOptionsDictionary)
        : this(new JsonSerializerOptions(), jsonOptionsDictionary)
    {
    }

    public JsonOptionsResolver(JsonSerializerOptions defaultJsonOptions, IReadOnlyDictionary<string, JsonSerializerOptions> jsonOptionsDictionary)
    {
        _defaultJsonOptions = defaultJsonOptions;
        _jsonOptionsDictionary = jsonOptionsDictionary;
    }

    public JsonSerializerOptions GetJsonOptions(string key)
    {
        if (_jsonOptionsDictionary.TryGetValue(key, out var jsonOptions))
            return jsonOptions;

        return _defaultJsonOptions;
    }
}
