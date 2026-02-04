// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using System.Text.Json.Serialization;

namespace DirectiveAthenaWeb.Content.Faq;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    WriteIndented = true
)]
[JsonSerializable(typeof(FaqContent[]), TypeInfoPropertyName = "FaqContentArray")]
internal partial class FaqContentJsonContext : JsonSerializerContext;
