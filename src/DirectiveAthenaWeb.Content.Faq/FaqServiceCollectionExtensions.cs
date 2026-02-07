// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Content;
using DirectiveAthenaWeb.Content.Faq;
using DirectiveAthenaWeb.DevServer.Pages;
using DirectiveAthenaWeb.Content.Faq.Resources;
using JetBrains.Annotations;
using Microsoft.Extensions.Localization;
using MudBlazor;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public static class FaqServiceCollectionExtensions {
    extension(IServiceCollection services) {
        [UsedImplicitly] public IServiceCollection AddFaqContent(out Assembly contentAssembly) {
            services.RegisterServicesFromDirectiveAthenaWebContentFaq();

            services.AddR2Storage<FaqContent>("faq");
            
            ContentEditorProvider.RegisterAtContentEditor<FaqContentEditor, IStringLocalizer<Faq>>(
                localizer => localizer[Faq.ContentManagerTabFaq],
                Icons.Material.Filled.Rule
            );
            
            ContentEditorProvider.RegisterAtTagsEditor<FaqContent>();
            contentAssembly = typeof(FaqServiceCollectionExtensions).Assembly;
            return services;
        }
    }

}
