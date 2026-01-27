// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthenaWeb.Services;
using DirectiveAthenaWeb.Services.ContentStorage;
using DirectiveAthenaWeb.Services.Writings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DirectiveAthenaWeb.Content.Writings.Services;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableScoped<IWritingRepository>]
public class WritingRepository(
    [FromKeyedServices(ContentCategory.Writings)] IContentStorage storage,
    ILogger<WritingRepository> logger
) : ContentRepository<Writing>(storage, logger), IWritingRepository;
