// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Services.ContentStorage;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public static class ServiceCollectionExtensions {
    extension(IServiceCollection services) {
        [UsedImplicitly] public IServiceCollection AddWritingContent() {
            services.RegisterServicesFromDirectiveAthenaWebContentFaq();

            services.AddContentStorage(ContentCategory.Faq);

            return services;
        }
    }

}
