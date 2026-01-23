// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;

namespace DirectiveAthena.Website.Services.Config;

// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public static class ServiceCollectionExtensions {
    public static IServiceCollection AddWebsiteServices(this IServiceCollection services) {
        services.AddInfiniBlazor(static config => {
            config.Components.SetRenderMode(RenderMode.InteractiveWebAssembly);
            config.Markdown.WithMudBlazorComponents();
        });

        services.RegisterServicesFromDirectiveAthenaWebsiteServices();
        
        return services;
    }
}
