// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthena.Website.Services.Localization;

namespace DirectiveAthena.Website.Services.FileSystem;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableScoped<IContentStorageFactory>]
public sealed class ContentStorageFactory(
    ILocalFileStorage fileStorage,
    ILocalizationProvider localizationProvider
) : IContentStorageFactory {
    private const string ContentRoot = "content";

    public IContentStorage ForCategory(ContentCategory category)
        => new ContentStorage(fileStorage, localizationProvider, GetCategoryFolder(category));

    private static string GetCategoryFolder(ContentCategory category)
        => category switch {
            ContentCategory.Articles => $"{ContentRoot}/articles",
            ContentCategory.WorldRules => $"{ContentRoot}/world-rules",
            _ => throw new ArgumentOutOfRangeException(nameof(category), category, "Unsupported content category.")
        };
}
