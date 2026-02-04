// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using System.Net;
using System.Net.Http.Headers;

namespace DirectiveAthenaWeb.Services.R2Storage;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public readonly record struct R2ReadResult(
    HttpStatusCode StatusCode,
    string? Content,
    EntityTagHeaderValue? ETag,
    DateTimeOffset? LastModifiedUtc
);
