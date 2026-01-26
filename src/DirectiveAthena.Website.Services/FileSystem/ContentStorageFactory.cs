// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthena.Website.Services.Localization;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace DirectiveAthena.Website.Services.FileSystem;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableScoped<IContentStorageFactory>]
public class ContentStorageFactory(
    ILocalizationProvider localizationProvider,
    IOptions<R2StorageOptions> r2Options,
    ILoggerFactory loggerFactory
) : IContentStorageFactory {
    private const string ContentRoot = "content";

    public IContentStorage ForCategory(ContentCategory category) {
        string folder = GetCategoryFolder(category);
        ILogger r2Logger = loggerFactory.CreateLogger<R2ContentStorage>();
        if (!r2Options.Value.IsReadConfigured) {
            r2Logger.Warning("R2 read configuration is missing; content reads may fail.");
        }

        r2Logger.Debug("Creating R2 content storage for {Category} at {Folder}.", category, folder);
        return new R2ContentStorage(localizationProvider, r2Options, folder, r2Logger);
    }

    private static string GetCategoryFolder(ContentCategory category)
        => category switch {
            ContentCategory.Articles => $"{ContentRoot}/articles",
            ContentCategory.WorldRules => $"{ContentRoot}/world-rules",
            _ => throw new ArgumentOutOfRangeException(nameof(category), category, "Unsupported content category.")
        };
}
