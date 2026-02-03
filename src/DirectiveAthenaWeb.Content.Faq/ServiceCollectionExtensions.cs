// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Content;
using DirectiveAthenaWeb.Content.Faq;
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
        [UsedImplicitly] public IServiceCollection AddFaqContent() {
            services.RegisterServicesFromDirectiveAthenaWebContentFaq();

            services.AddContentStorage<FaqContent>("faq");
            
            ContentEditorProvider.Register<FaqContentEditor, IStringLocalizer<Shared>>(
                localizer => localizer[Shared.ContentManagerTabFaq],
                Icons.Material.Filled.Rule
            );
            
            return services;
        }
    }

}
