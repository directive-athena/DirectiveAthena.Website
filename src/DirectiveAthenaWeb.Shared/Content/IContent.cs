// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
namespace DirectiveAthenaWeb.Content;

// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public interface IContent {
    Guid Id { get; }
    
    string InternalTitle { get; }
    IReadOnlyCollection<string> Tags { get; }
    
    DateTime HiddenAt { get; }
    DateTime SoftDeletedAt { get; }
    DateTime CreatedAt { get; }
    DateTime LastModifiedAt { get; }
    
    bool IsDevContent { get; }
    bool IsHidden { get; }
    bool IsSoftDeleted { get; }
    string MarkdownFileName { get; }
    
    bool AddTag(string tag);
    bool RemoveTag(string tag);
    
    string GetReadableInternalTitle();
}
