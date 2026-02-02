// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Services.Content;
using System.Text.Json.Serialization;

namespace DirectiveAthenaWeb.Content;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public abstract class ContentBase : IContent {
    public Guid Id { get; set; } = Guid.CreateVersion7();

    [JsonIgnore] private readonly HashSet<string> _tags = [];
    [JsonIgnore] public IReadOnlyCollection<string> Tags {
        get => _tags.AsReadOnly();
        init {
            _tags.Clear();
            _tags.UnionWith(value);
        }
    }
    [JsonPropertyName("tags")] public List<string> TagsSerialized {
        get => _tags.ToList();
        set {
            _tags.Clear();
            _tags.UnionWith(value);
        }
    }
    
    public DateTime HiddenAt { get; set; } = DateTime.MinValue;
    public DateTime SoftDeletedAt { get; set; } = DateTime.MinValue;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastModifiedAt { get; set; } = DateTime.MinValue;

    [JsonIgnore] public bool IsHidden => HiddenAt > DateTime.MinValue;
    [JsonIgnore] public bool IsSoftDeleted => SoftDeletedAt > DateTime.MinValue;
    [JsonIgnore] public string MarkdownFileName => $"{Id:D}.md";

    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    public bool AddTag(string tag) {
        if (!_tags.Add(tag)) return false;

        LastModifiedAt = DateTime.UtcNow;
        return true;
    }

    public bool RemoveTag(string tag) {
        if (!_tags.Remove(tag)) return false;

        LastModifiedAt = DateTime.UtcNow;
        return true;
    }
}