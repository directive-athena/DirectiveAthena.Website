// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using JetBrains.Annotations;
using System.Text.Json.Serialization;

namespace DirectiveAthenaWeb.Content;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public abstract class ContentBase : IContent {
    public required Guid Id { get; set; }
    public required string InternalTitle { get; set; }
    
    public LocalizedDataHolder LocalizedTitles { get; init; } = new();
    public LocalizedDataHolder LocalizedSummaries { get; init; } = new();
    
    [JsonIgnore] private readonly HashSet<string> _tags = [];
    [JsonIgnore] public IReadOnlyCollection<string> Tags {
        get => _tags.AsReadOnly();
        init {
            _tags.Clear();
            _tags.UnionWith(value);
        }
    }
    [UsedImplicitly, JsonPropertyName("tags")] public List<string> TagsSerialized {
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

    public bool IsDevContent { get; set; }
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

    public string GetReadableInternalTitle() 
        => InternalTitle.IsNotNullOrWhiteSpace() ? InternalTitle : Id.ToString();
}