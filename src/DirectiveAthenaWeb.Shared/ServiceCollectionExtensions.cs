// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Services.Content;
using DirectiveAthenaWeb.Services.ContentStorage;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public static class ServiceCollectionExtensions {
    extension(IServiceCollection services) {
        public void AddContentStorage<TContent>(string category) where TContent : IContent {
            services.AddKeyedScoped<IContentStorage>(
                category,
                static (provider, key) => {
                    string? category = key as string;
                    ArgumentNullException.ThrowIfNull(category);

                    var factory = provider.GetRequiredService<IContentStorageFactory>();
                    return factory.ForCategory(category.ToLowerInvariant());
                }
            );
            
            services.AddKeyedScoped<IContentStorage>(
                typeof(TContent),
                 (provider, _) => {
                    var factory = provider.GetRequiredService<IContentStorageFactory>();
                    return factory.ForCategory(category.ToLowerInvariant());
                }
            );
        }
    }

}
