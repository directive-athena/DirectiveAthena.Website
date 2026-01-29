// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthenaWeb.Services.Content;
using DirectiveAthenaWeb.Services.ContentStorage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DirectiveAthenaWeb.Content.Faq.Services;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableScoped<IFaqContentRepository>]
[InjectableScoped<IContentRepository<FaqContent>>]
internal class FaqContentRepository(
    [FromKeyedServices("faq")] IContentStorage storage,
    ILogger<FaqContentRepository> logger
) : ContentRepository<FaqContent>(storage, logger), IFaqContentRepository;
