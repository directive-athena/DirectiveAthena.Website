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
[InjectableSingleton<IContentEditorProvider>]
public class ContentEditorProvider : IContentEditorProvider {
    private static readonly List<ContentEditorInfo> RegisteredEditors = new();
    
    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    public static void Register<TContentEditor, TLocalizer>(Func<TLocalizer, LocalizedString> getLocalizedTitle, string icon) where TLocalizer : notnull {
        var normalizedFunc = new Func<IServiceProvider, string>(provider => getLocalizedTitle(provider.GetRequiredService<TLocalizer>()));
        Type normalizedType = typeof(TContentEditor);
        
        RegisteredEditors.Add(new ContentEditorInfo(normalizedType, normalizedFunc, icon));
    }
    
    public IReadOnlyCollection<ContentEditorInfo> GetEditors() => RegisteredEditors.AsReadOnly();
    
}
