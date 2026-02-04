// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthenaWeb.Services.R2Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization.Metadata;

namespace DirectiveAthenaWeb.Content.Note.Services;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableScoped<INoteContentRepository>]
[InjectableScoped<IContentRepository<NoteContent>>]
internal class NoteContentRepository(
    [FromKeyedServices("note")] IContentStorage storage,
    ILogger<NoteContentRepository> logger
) : ContentRepositoryBase<NoteContent>(storage, logger), INoteContentRepository {
    protected override JsonTypeInfo<NoteContent[]> ContentListTypeInfo
        => NoteContentJsonContext.Default.NoteContentArray;
}
