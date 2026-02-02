// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthenaWeb.Services.Content;
using DirectiveAthenaWeb.Services.ContentStorage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DirectiveAthenaWeb.Content.Note.Services;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableScoped<INoteContentRepository>]
[InjectableScoped<IContentRepository<NoteContent>>]
internal class NoteContentRepository(
    [FromKeyedServices("note")] IContentStorage storage,
    ILogger<NoteContentRepository> logger
) : ContentRepositoryBase<NoteContent>(storage, logger), INoteContentRepository;
