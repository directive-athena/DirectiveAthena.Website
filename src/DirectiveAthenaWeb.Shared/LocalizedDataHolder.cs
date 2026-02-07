// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace DirectiveAthenaWeb;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[JsonConverter(typeof(LocalizedDataHolderJsonConverter))]
public class LocalizedDataHolder {
    private readonly ConcurrentDictionary<string, string> _values = new(StringComparer.OrdinalIgnoreCase);
    [JsonIgnore] public IReadOnlyDictionary<string, string> Values => _values;
    [JsonIgnore] public IEnumerable<string> Keys => _values.Keys;
    [JsonIgnore] public int Count => _values.Count;
    [JsonIgnore] public bool IsEmpty => _values.IsEmpty;
    

    [JsonIgnore] public string this[string code] {
        get => _values.GetValueOrDefault(code) ?? string.Empty;
        set => _values.AddOrUpdate(code, value => value, (_, _) => value);
    }
    // -----------------------------------------------------------------------------------------------------------------
    // Constructors
    // -----------------------------------------------------------------------------------------------------------------
    public LocalizedDataHolder() {}
    public LocalizedDataHolder(IDictionary<string, string> values) {
        foreach ((string k, string v) in values) _values[k] = v;
    }
    
    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------

    public bool TryGetWithFallback(string code, string fallback, [NotNullWhen(true)] out string? value)
        => _values.TryGetValue(code, out value) || _values.TryGetValue(fallback, out value);

    public void Set(string code, string value) {
        _values[code] = value;
    }
    public void Remove(string code) {
        _values.TryRemove(code, out _);
    }
    
    public static LocalizedDataHolder FromDictionary(Dictionary<string, string> dict) {
        ArgumentNullException.ThrowIfNull(dict);

        var holder = new LocalizedDataHolder();
        foreach ((string key, string value) in dict) {
            holder._values.TryAdd(key, value);
        }

        return holder;
    }
}
