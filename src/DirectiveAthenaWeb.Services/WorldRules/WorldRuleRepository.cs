// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthenaWeb.Services.ContentStorage;
using Microsoft.Extensions.Logging;

namespace DirectiveAthenaWeb.Services.WorldRules;
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
