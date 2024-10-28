using System.Globalization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace Presentation.Services;


public class JsonStringLocalizer : IStringLocalizer
{
    private readonly IDistributedCache _cache;

    private readonly JsonSerializer _serializer = new JsonSerializer();

    public JsonStringLocalizer(IDistributedCache cache)
    {
        _cache = cache;
    }

    public LocalizedString this[string name]
    {
        get
        {
            var value = GetString(name);

            return new LocalizedString(name, value ?? name, value is null);
        }
    }

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            var actualValue = this[name];

            return !actualValue.ResourceNotFound
                ? new LocalizedString(name, string.Format(actualValue.Value, arguments), false)
                : actualValue;
        }
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        var filePath = $"Resources/{Thread.CurrentThread.CurrentCulture.Name}.json";

        using var str = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var streamReader = new StreamReader(str, encoding: System.Text.Encoding.UTF8);
        using var reader = new JsonTextReader(streamReader);
        while (reader.Read())
        {
            if (reader.TokenType != JsonToken.PropertyName)
            {
                continue;
            }

            var key = (string)reader.Value!;

            reader.Read();

            var value = _serializer.Deserialize<string>(reader)!;

            yield return new LocalizedString(key, value, false);
        }
    }

    private string GetString(string key)
    {
        var relativeFilePath = $"Resources/{Thread.CurrentThread.CurrentCulture.Name}.json";
        var fullFilePath = Path.GetFullPath(relativeFilePath);

        if (!File.Exists(fullFilePath)) return string.Empty;
        
        var keyCulture = CultureInfo.CurrentUICulture;

        var cacheKey = $"locale:name={key}&culture={keyCulture.Name}";

        var cacheValue = _cache.GetString(cacheKey)!;

        if (!string.IsNullOrEmpty(cacheValue))
        {
            return cacheValue;
        }

        var result = GetValueFromJSON(key, Path.GetFullPath(relativeFilePath));

        if (string.IsNullOrEmpty(result))
        {
            return $"{key} message cannot be found in {relativeFilePath}";
        }

        _cache.SetString(cacheKey, result);

        return result;
    }

    private string GetValueFromJSON(string propertyName, string filePath)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            throw new ArgumentNullException(nameof(propertyName));
        }

        if (string.IsNullOrEmpty(filePath))
        {
            throw new ArgumentNullException(nameof(filePath));
        }

        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var streamReader = new StreamReader(fileStream, encoding: System.Text.Encoding.UTF8);
        using var reader = new JsonTextReader(streamReader);
        while (reader.Read())
        {
            if (reader.TokenType == JsonToken.PropertyName && string.Equals((string)reader.Value!, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                reader.Read();
                return _serializer.Deserialize<string>(reader)!;
            }
        }

        return string.Empty;
    }
}
