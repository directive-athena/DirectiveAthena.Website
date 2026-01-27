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
        public void AddContentStorage(ContentCategory category)
            => services.AddKeyedScoped<IContentStorage>(
                category,
                (provider, key) => provider.GetRequiredService<IContentStorageFactory>().ForCategory((ContentCategory)(key ?? throw new ArgumentNullException(nameof(key))))
            );
    }

}
