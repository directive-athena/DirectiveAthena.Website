// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Services.ContentStorage;
using DirectiveAthenaWeb.Services.Markdown;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;

namespace DirectiveAthenaWeb.Services;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public static class ServiceCollectionExtensions {
    extension(IServiceCollection services) {
        public IServiceCollection AddWebsiteServices() {
            services.AddInfiniBlazor(static config => {
                config.Components.SetRenderMode(RenderMode.InteractiveWebAssembly);
                config.Markdown.WithMudBlazorComponents();
            });

            services.RegisterServicesFromDirectiveAthenaWebServices();
            services.RegisterServicesFromDirectiveAthenaWebServicesLocalization();

            services.AddContentStorage(ContentCategory.Articles);
            services.AddContentStorage(ContentCategory.WorldRules);

            return services;
        }
        
        private IServiceCollection AddContentStorage(ContentCategory category)
            => services.AddKeyedScoped<IContentStorage>(
                category,
                (provider, key) => provider.GetRequiredService<IContentStorageFactory>().ForCategory((ContentCategory)(key ?? throw new ArgumentNullException(nameof(key))))
            );
    }

}
