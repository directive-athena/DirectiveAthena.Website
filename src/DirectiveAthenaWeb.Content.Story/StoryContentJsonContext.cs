// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using System.Text.Json.Serialization;

namespace DirectiveAthenaWeb.Content.Story;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    WriteIndented = true
)]
[JsonSerializable(typeof(StoryContent[]), TypeInfoPropertyName = "StoryContentArray")]
internal partial class StoryContentJsonContext : JsonSerializerContext;
