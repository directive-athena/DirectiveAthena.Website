// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace DirectiveAthenaWeb.Content;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public static class ServiceCollectionExtensions {
    extension(IServiceCollection services) {
        
        [UsedImplicitly] public IServiceCollection AddWebsiteContent() {
            services.RegisterServicesFromDirectiveAthenaWebContent();
            return services;
        }
    }

}
