// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
namespace DirectiveAthenaWeb.Content;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public readonly record struct ContentFilter(
    string? TitleQuery = null,
    IReadOnlyCollection<string>? Tags = null
) {
    public static ContentFilter Empty => new();
    public bool IsEmpty => TitleQuery is null && Tags is null or {Count: 0};
}
