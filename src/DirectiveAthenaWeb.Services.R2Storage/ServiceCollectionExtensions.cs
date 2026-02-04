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
    private static readonly Dictionary<Type, string> CategoryMap = new();
    
    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    extension(IServiceCollection services) {
        public void AddR2Storage<TContent>(string category) where TContent : IContent {
            CategoryMap.AddOrUpdate(typeof(TContent), category);
            
            services.AddKeyedScoped<IR2Storage>(
                category,
                static (provider, key) => {
                    if (key is not string category) throw new ArgumentException("Key must be of type string");

                    var factory = provider.GetRequiredService<IR2StorageFactory>();
                    return factory.ForCategory(category.ToLowerInvariant());
                }
            );
            
            services.AddKeyedScoped<IR2Storage>(
                typeof(TContent),
                static (provider, key) => {
                    if (key is not Type type) throw new ArgumentException("Key must be of type Type");
                    string category = CategoryMap[type];
                    return provider.GetRequiredKeyedService<IR2Storage>(category);
                }
            );
        }
    }

}
