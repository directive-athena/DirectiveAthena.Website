// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Content;
using DirectiveAthenaWeb.Content.Story;
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
        [UsedImplicitly] public IServiceCollection AddStoryContent() {
            services.RegisterServicesFromDirectiveAthenaWebContentStory();

            services.AddContentStorage<StoryContent>("story");
            
            ContentEditorProvider.Register<StoryContentEditor, IStringLocalizer<Shared>>(
                localizer => localizer[Shared.ContentManagerTabStory],
                Icons.Material.Filled.AutoStories
            );


            
            
            return services;
        }
    }

}
