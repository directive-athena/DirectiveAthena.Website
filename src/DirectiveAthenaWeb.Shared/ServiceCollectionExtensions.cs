// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Services.ContentStorage;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public static class ServiceCollectionExtensions {
    extension(IServiceCollection services) {
        public void AddContentStorage(string category)
            => services.AddKeyedScoped<IContentStorage>(
                category,
                static (provider, key) => {
                    string? category = key as string;
                    ArgumentNullException.ThrowIfNull(category);
                    
                    var factory = provider.GetRequiredService<IContentStorageFactory>();
                    return factory.ForCategory(category.ToLowerInvariant());
                }
                    
            );
    }

}
