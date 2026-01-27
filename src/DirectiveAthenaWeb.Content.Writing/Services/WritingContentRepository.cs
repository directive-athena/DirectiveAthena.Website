// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthenaWeb.Services.Content;
using DirectiveAthenaWeb.Services.ContentStorage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DirectiveAthenaWeb.Content.Writing.Services;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableScoped<IWritingContentRepository>]
public class WritingContentRepository(
    [FromKeyedServices(ContentCategory.Writing)] IContentStorage storage,
    ILogger<WritingContentRepository> logger
) : ContentRepository<WritingContent>(storage, logger), IWritingContentRepository;
