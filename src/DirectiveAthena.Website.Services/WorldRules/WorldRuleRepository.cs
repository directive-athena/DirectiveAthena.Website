// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthena.Website.Services.FileSystem;

namespace DirectiveAthena.Website.Services.WorldRules;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableScoped<IWorldRuleRepository>]
public class WorldRuleRepository(
    HttpClient http,
    IContentStorageFactory storageFactory
) : ContentRepository<WorldRule>(
    http,
    storageFactory.ForCategory(ContentCategory.WorldRules)
), IWorldRuleRepository {
    protected override string IndexPath => Storage.IndexContentPath;
}
