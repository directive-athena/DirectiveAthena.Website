// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using System.Net;
using System.Net.Http.Headers;

namespace DirectiveAthenaWeb.Services.ContentStorage;

// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public readonly record struct ContentReadResult(
    HttpStatusCode StatusCode,
    string? Content,
    EntityTagHeaderValue? ETag,
    DateTimeOffset? LastModifiedUtc
) {
    public bool IsSuccessStatusCode => (int)StatusCode is >= 200 and <= 299;
}
