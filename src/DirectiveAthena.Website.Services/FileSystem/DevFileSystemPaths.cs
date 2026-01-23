// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthena.Website.Services.Localization;

namespace DirectiveAthena.Website.Services.FileSystem;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableSingleton<IDevFileSystemPaths>]
public class DevFileSystemPaths(ILocalizationProvider localizationProvider) : IDevFileSystemPaths{

    public string GetIndexPath()
        => "src/DirectiveAthena.Website/wwwroot/content/articles/index.json";

    public string GetMarkdownPath(string locale, string fileName)
        => $"src/DirectiveAthena.Website/wwwroot/content/articles/{locale}/{fileName}";

    public string GetWorldRulesIndexPath()
        => "src/DirectiveAthena.Website/wwwroot/content/world-rules/index.json";

    public string GetWorldRuleMarkdownPath(string locale, string fileName)
        => $"src/DirectiveAthena.Website/wwwroot/content/world-rules/{locale}/{fileName}";

    public string GetSharedResxPath(string locale)
        => locale == localizationProvider.DefaultLocalization.Code
            ? "src/DirectiveAthena.Website/Resources/Shared.resx"
            : $"src/DirectiveAthena.Website/Resources/Shared.{locale}.resx";

    public string GetTagsResxPath(string locale)
        => locale == localizationProvider.DefaultLocalization.Code
            ? "src/DirectiveAthena.Website/Resources/Tags.resx"
            : $"src/DirectiveAthena.Website/Resources/Tags.{locale}.resx";
}
