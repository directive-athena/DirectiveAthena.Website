// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using System.Text.Json.Serialization;

namespace DirectiveAthenaWeb.Services.R2Storage;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(ProxyUploadRequest))]
[JsonSerializable(typeof(ProxyDeleteRequest))]
internal partial class R2StorageJsonContext : JsonSerializerContext;
