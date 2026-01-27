// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthena.Website.Services.ContentStorage;
using Microsoft.Extensions.Logging;

namespace DirectiveAthena.Website.Services.WorldRules;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableScoped<IWorldRuleRepository>]
public class WorldRuleRepository(
    IContentStorageFactory storageFactory,
    ILogger<WorldRuleRepository> logger
) : ContentRepository<WorldRule>(
        storageFactory.ForCategory(ContentCategory.WorldRules),
        logger
    ),
    IWorldRuleRepository {
    protected override string IndexPath => Storage.IndexContentPath;
}
