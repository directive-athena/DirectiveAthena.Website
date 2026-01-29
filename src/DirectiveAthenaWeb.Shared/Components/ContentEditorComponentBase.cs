// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Services.Content;
using Microsoft.AspNetCore.Components;

namespace DirectiveAthenaWeb.Components;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public abstract class ContentEditorComponentBase<TContent> : WebsiteComponentBase where TContent : ContentBase {
    protected TContent Content { get; set; } = null!;
    [Parameter] public EventCallback OnContentChanged { get; set; }
    [Parameter] public EventCallback OnTagsChanged { get; set; }

    protected void SetLocalizedValue(Dictionary<string, string> values, string culture, string value) {
        if (values.TryGetValue(culture, out string? existing) && existing == value) return;

        values[culture] = value;
        TouchLastModified();
        NotifyContentChanged();
    }

    protected void TouchLastModified() {
        Content.LastModifiedAt = DateTime.UtcNow;
    }

    protected void NotifyTagsChanged() {
        if (OnTagsChanged.HasDelegate) {
            _ = OnTagsChanged.InvokeAsync();
        }
    }

    protected void NotifyContentChanged() {
        if (OnContentChanged.HasDelegate) {
            _ = OnContentChanged.InvokeAsync();
        }
    }
}
