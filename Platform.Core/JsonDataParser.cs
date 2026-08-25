using System.Text.Json;

namespace Platform.Core.Parsing;

public static class JsonDataParser
{
    /// <summary>
    /// Разбирает произвольный JSON в словарь. Парсер сохранён как возможный адаптер
    /// будущего внешнего API, но не участвует в текущем цикле выдачи достижений.
    /// </summary>
    public static Dictionary<string, object?> ParseToDictionary(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new ArgumentException("JSON string is empty.", nameof(json));

        using var doc = JsonDocument.Parse(json);

        if (doc.RootElement.ValueKind != JsonValueKind.Object)
            throw new FormatException("Root JSON element must be an object.");

        return (Dictionary<string, object?>)ToObject(doc.RootElement)!;
    }

    private static object? ToObject(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
            {
                var dict = new Dictionary<string, object?>(StringComparer.Ordinal);
                foreach (var prop in element.EnumerateObject())
                    dict[prop.Name] = ToObject(prop.Value);
                return dict;
            }

            case JsonValueKind.Array:
            {
                var list = new List<object?>();
                foreach (var item in element.EnumerateArray())
                    list.Add(ToObject(item));
                return list;
            }

            case JsonValueKind.String:
                return element.GetString();

            case JsonValueKind.Number:
                if (element.TryGetInt64(out var l)) return l;
                if (element.TryGetDecimal(out var d)) return d;
                return element.GetDouble();

            case JsonValueKind.True:
                return true;

            case JsonValueKind.False:
                return false;

            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                return null;

            default:
                return null;
        }
    }
}
