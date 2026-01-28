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
        [UsedImplicitly] public IServiceCollection AddNoteContent() {
            services.RegisterServicesFromDirectiveAthenaWebContentNote();

            services.AddContentStorage("note");

            return services;
        }
    }

}
