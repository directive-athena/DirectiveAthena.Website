// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthenaWeb.Services.Content;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace DirectiveAthenaWeb.Content;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableScoped<IContentEditorProvider>]
public class ContentEditorProvider(IServiceProvider provider) : IContentEditorProvider {
    private static readonly List<ContentEditorInfo> RegisteredEditors = new();
    private static readonly List<Func<IServiceProvider, Task<IEnumerable<IContent>>>> RegisteredTagRetrievers = new();

    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    public static void RegisterAtTagsEditor<TContent>() where TContent : IContent {
        var normalizedFunc = new Func<IServiceProvider, Task<IEnumerable<IContent>>>(static async provider => {
            const QueryConfig config = QueryConfig.WithHidden | QueryConfig.WithSoftDeleted;
            
            var repo = provider.GetRequiredService<IContentRepository<TContent>>();
            TContent[] tags = await repo.GetAllAsync(config);
            return tags.Cast<IContent>();
        });

        RegisteredTagRetrievers.Add(normalizedFunc);
    }

    public static void RegisterAtContentEditor<TContentEditor, TLocalizer>(Func<TLocalizer, LocalizedString> getLocalizedTitle, string icon) where TLocalizer : notnull {
        var normalizedFunc = new Func<IServiceProvider, string>(provider => getLocalizedTitle(provider.GetRequiredService<TLocalizer>()));
        Type normalizedType = typeof(TContentEditor);

        RegisteredEditors.Add(new ContentEditorInfo(normalizedType, normalizedFunc, icon));
    }

    public IReadOnlyCollection<ContentEditorInfo> GetEditors() => RegisteredEditors.AsReadOnly();
    public IEnumerable<Func<Task<IEnumerable<IContent>>>> GetTagRetrievers() => RegisteredTagRetrievers.Select(retriever => (Func<Task<IEnumerable<IContent>>>)(() => retriever(provider)));

}
