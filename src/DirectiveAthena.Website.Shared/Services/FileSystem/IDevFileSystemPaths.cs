// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
namespace DirectiveAthena.Website.Services.FileSystem;

// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public interface IDevFileSystemPaths {
    string GetIndexPath();
    string GetMarkdownPath(string locale, string fileName);
    string GetWorldRulesIndexPath();
    string GetWorldRuleMarkdownPath(string locale, string fileName);
    string GetSharedResxPath(string locale);
    string GetTagsResxPath(string locale);
}
