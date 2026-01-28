// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public static class ServiceCollectionExtensions {
    extension(IServiceCollection services) {
        [UsedImplicitly] public IServiceCollection AddStoryContent() {
            services.RegisterServicesFromDirectiveAthenaWebContentStory();

            services.AddContentStorage("story");

            return services;
        }
    }

}
