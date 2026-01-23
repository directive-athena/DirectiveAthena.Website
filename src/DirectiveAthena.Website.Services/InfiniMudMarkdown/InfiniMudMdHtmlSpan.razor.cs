// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using System.Text.RegularExpressions;

namespace DirectiveAthena.Website.Services.InfiniMudMarkdown;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public partial class InfiniMudMdHtmlSpan {
    [GeneratedRegex("""style\s*=\s*["']([^"']*)["']""", RegexOptions.IgnoreCase)]
    private static partial Regex ExtractStyleAttributeRegex { get; }
    
    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    private static string? ExtractStyleAttribute(string? htmlTag) {
        if (htmlTag.IsNullOrEmpty()) return null;
        try {
            Match match = ExtractStyleAttributeRegex.Match(htmlTag);
            return match.Success
                ? match.Groups[1].Value
                : null;
        }
        catch (Exception) {
            return null;
        }
    }
}
