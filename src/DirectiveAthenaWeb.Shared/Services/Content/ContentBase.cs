// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using System.Text.Json.Serialization;

namespace DirectiveAthenaWeb.Services.Content;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public abstract class ContentBase {
    public Guid Id { get; set; }
    public DateTime HiddenAt { get; set; } = DateTime.MinValue;
    public DateTime SoftDeletedAt { get; set; } = DateTime.MinValue;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastModifiedAt { get; set; } = DateTime.MinValue;
    
    [JsonIgnore] public bool IsHidden => HiddenAt > DateTime.MinValue;
    [JsonIgnore] public bool IsSoftDeleted => SoftDeletedAt > DateTime.MinValue;
    [JsonIgnore] public string MarkdownFileName => $"{Id:D}.md";
}
