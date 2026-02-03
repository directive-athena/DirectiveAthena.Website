// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Content;
using DirectiveAthenaWeb.Content.Note;
using DirectiveAthenaWeb.DevServer.Pages;
using DirectiveAthenaWeb.Services.Localization.Resources;
using JetBrains.Annotations;
using Microsoft.Extensions.Localization;
using MudBlazor;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public static class ServiceCollectionExtensions {
    extension(IServiceCollection services) {
        [UsedImplicitly] public IServiceCollection AddNoteContent() {
            services.RegisterServicesFromDirectiveAthenaWebContentNote();

            services.AddContentStorage<NoteContent>("note");
            
            ContentEditorProvider.RegisterAtContentEditor<NoteContentEditor, IStringLocalizer<Shared>>(
                localizer => localizer[Shared.ContentManagerTabStory],
                Icons.Material.Filled.Article
            );
            
            ContentEditorProvider.RegisterAtTagsEditor<NoteContent>();

            return services;
        }
    }

}
