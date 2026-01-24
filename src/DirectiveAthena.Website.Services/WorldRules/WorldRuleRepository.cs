// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthena.Website.Services.FileSystem;
using DirectiveAthena.Website.Services.Localization;

namespace DirectiveAthena.Website.Services.WorldRules;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableScoped<IWorldRuleRepository>]
public class WorldRuleRepository(
    HttpClient http,
    IDevFileSystemManager devFs,
    ILocalizationProvider localizationProvider,
    IDevFileSystemPaths devFsPaths
) : ContentRepository<WorldRule>(
    http,
    new DevFileSystemCategoryManager(devFs, devFsPaths, localizationProvider, DevFileSystemCategory.WorldRules)
), IWorldRuleRepository {
    
    protected override string IndexPath => "content/world-rules/index.json";
}
