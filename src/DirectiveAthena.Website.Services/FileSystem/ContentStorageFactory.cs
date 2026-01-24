// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthena.Website.Services.Localization;
using Microsoft.Extensions.Logging;

namespace DirectiveAthena.Website.Services.FileSystem;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableScoped<IContentStorageFactory>]
public class ContentStorageFactory(
    ILocalFileStorage fileStorage,
    ILocalizationProvider localizationProvider,
    ILoggerFactory loggerFactory
) : IContentStorageFactory {
    private const string ContentRoot = "content";

    public IContentStorage ForCategory(ContentCategory category) {
        string folder = GetCategoryFolder(category);
        ILogger storageLogger = loggerFactory.CreateLogger<ContentStorage>();
        storageLogger.Debug("Creating content storage for {Category} at {Folder}.", category, folder);
        return new ContentStorage(fileStorage, localizationProvider, folder, storageLogger);
    }

    private static string GetCategoryFolder(ContentCategory category)
        => category switch {
            ContentCategory.Articles => $"{ContentRoot}/articles",
            ContentCategory.WorldRules => $"{ContentRoot}/world-rules",
            _ => throw new ArgumentOutOfRangeException(nameof(category), category, "Unsupported content category.")
        };
}
