// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Content;
using DirectiveAthenaWeb.Content.Story;
using DirectiveAthenaWeb.DevServer.Pages;
using DirectiveAthenaWeb.Content.Story.Resources;
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
        [UsedImplicitly] public IServiceCollection AddStoryContent() {
            services.RegisterServicesFromDirectiveAthenaWebContentStory();

            services.AddContentStorage<StoryContent>("story");
            
            ContentEditorProvider.RegisterAtContentEditor<StoryContentEditor, IStringLocalizer<Story>>(
                localizer => localizer[Story.ContentManagerTabStory],
                Icons.Material.Filled.AutoStories
            );

            ContentEditorProvider.RegisterAtTagsEditor<StoryContent>();
            
            return services;
        }
    }

}
