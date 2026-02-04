// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Content;
using DirectiveAthenaWeb.Services.R2Storage;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public static class ServiceCollectionExtensions {
    
    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    extension(IServiceCollection services) {
        public void AddR2Storage<TContent>(string category) where TContent : IContent {
            R2ContentStorageFactory.CategoryMap.AddOrUpdate(typeof(TContent), category);

            services.AddScoped<IR2Storage<TContent>>(static provider => provider.GetRequiredService<IR2StorageFactory>().ForCategory<TContent>()); 
        }
    }

}
