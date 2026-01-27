// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthenaWeb.Services;
using DirectiveAthenaWeb.Services.ContentStorage;
using DirectiveAthenaWeb.Services.WorldFaq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DirectiveAthenaWeb.Content.Faq.Services;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableScoped<IWorldFaqRepository>]
public class WorldFaqRepository(
    [FromKeyedServices(ContentCategory.WorldFaq)] IContentStorage storage,
    ILogger<WorldFaqRepository> logger
) : ContentRepository<WorldFaq>(storage, logger), IWorldFaqRepository;
