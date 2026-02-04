// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthenaWeb.Services.R2Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization.Metadata;

namespace DirectiveAthenaWeb.Content.Faq.Services;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableScoped<IFaqContentRepository>]
[InjectableScoped<IContentRepository<FaqContent>>]
internal class FaqContentRepository(
    [FromKeyedServices("faq")] IContentStorage storage,
    ILogger<FaqContentRepository> logger
) : ContentRepositoryBase<FaqContent>(storage, logger), IFaqContentRepository {
    protected override JsonTypeInfo<FaqContent[]> ContentListTypeInfo
        => FaqContentJsonContext.Default.FaqContentArray;
}
