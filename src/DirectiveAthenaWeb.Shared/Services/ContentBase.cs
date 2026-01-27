// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using System.Text.Json.Serialization;

namespace DirectiveAthenaWeb.Services;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public abstract class ContentBase {
    public Guid Id { get; set; }
    public bool IsHidden { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastModifiedAt { get; set; } = DateTime.MinValue;
    
    [JsonIgnore]
    public string MarkdownFileName => $"{Id:D}.md";
}
