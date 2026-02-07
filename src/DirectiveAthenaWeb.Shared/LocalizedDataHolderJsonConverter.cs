// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
 
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DirectiveAthenaWeb;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public sealed class LocalizedDataHolderJsonConverter : JsonConverter<LocalizedDataHolder> {
    public override LocalizedDataHolder Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException("LocalizedDataHolder expects a JSON object.");

        var holder = new LocalizedDataHolder();
        while (reader.Read()) {
            if (reader.TokenType == JsonTokenType.EndObject) return holder;

            if (reader.TokenType != JsonTokenType.PropertyName) throw new JsonException("LocalizedDataHolder expects property names.");

            string key = reader.GetString() ?? string.Empty;
            if (!reader.Read()) throw new JsonException("LocalizedDataHolder expects a value.");

            string value = reader.TokenType switch {
                JsonTokenType.String => reader.GetString() ?? string.Empty,
                JsonTokenType.Null => string.Empty,
                _ => JsonDocument.ParseValue(ref reader).RootElement.ToString()
            };

            holder.Set(key, value);
        }

        throw new JsonException("LocalizedDataHolder expects an end object token.");
    }

    public override void Write(Utf8JsonWriter writer, LocalizedDataHolder value, JsonSerializerOptions options) {
        writer.WriteStartObject();
        foreach ((string key, string text) in value.Values) {
            writer.WriteString(key, text);
        }
        writer.WriteEndObject();
    }
}
