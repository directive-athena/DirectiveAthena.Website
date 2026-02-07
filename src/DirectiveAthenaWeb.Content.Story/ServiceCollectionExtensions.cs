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
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public static class ServiceCollectionExtensions {
    extension(IServiceCollection services) {
        [UsedImplicitly] public IServiceCollection AddStoryContent(out Assembly contentAssembly) {
            services.RegisterServicesFromDirectiveAthenaWebContentStory();

            services.AddR2Storage<StoryContent>("story");
            
            ContentEditorProvider.RegisterAtContentEditor<StoryContentEditor, IStringLocalizer<Story>>(
                localizer => localizer[Story.ContentManagerTabStory],
                Icons.Material.Filled.AutoStories
            );

            ContentEditorProvider.RegisterAtTagsEditor<StoryContent>();

            contentAssembly = typeof(ServiceCollectionExtensions).Assembly;
            return services;
        }
    }

}
