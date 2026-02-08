// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Content;
using DirectiveAthenaWeb.Content.Note;
using DirectiveAthenaWeb.DevServer.Pages;
using DirectiveAthenaWeb.Content.Note.Resources;
using JetBrains.Annotations;
using Microsoft.Extensions.Localization;
using MudBlazor;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public static class NoteServiceCollectionExtensions {
    extension(IServiceCollection services) {
        [UsedImplicitly] public IServiceCollection AddNoteContent(out Assembly contentAssembly) {
            services.RegisterServicesFromDirectiveAthenaWebContentNote();

            services.AddR2Storage<NoteContent>("note");
            
            ContentEditorProvider.RegisterAtContentEditor<NoteContentEditor, IStringLocalizer<Note>>(
                localizer => localizer[Note.ContentManagerTabWritings],
                Icons.Material.Filled.Article
            );
            
            ContentEditorProvider.RegisterAtTagsEditor<NoteContent>();

            contentAssembly = typeof(NoteServiceCollectionExtensions).Assembly;
            return services;
        }
    }

}
