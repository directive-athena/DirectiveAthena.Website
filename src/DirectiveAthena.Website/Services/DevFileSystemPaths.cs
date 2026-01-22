// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthena.Website.Models;

namespace DirectiveAthena.Website.Services;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public static class DevFileSystemPaths {

    public static string GetIndexPath()
        => "src/DirectiveAthena.Website/wwwroot/content/articles/index.json";

    public static string GetMarkdownPath(string locale, string fileName)
        => $"src/DirectiveAthena.Website/wwwroot/content/articles/{locale}/{fileName}";

    public static string GetSharedResxPath(string locale)
        => locale == LocalizationProvider.DefaultLocalization.Code
            ? "src/DirectiveAthena.Website/Resources/Shared.resx"
            : $"src/DirectiveAthena.Website/Resources/Shared.{locale}.resx";

    public static string GetTagsResxPath(string locale)
        => locale == LocalizationProvider.DefaultLocalization.Code
            ? "src/DirectiveAthena.Website/Resources/Tags.resx"
            : $"src/DirectiveAthena.Website/Resources/Tags.{locale}.resx";
}
