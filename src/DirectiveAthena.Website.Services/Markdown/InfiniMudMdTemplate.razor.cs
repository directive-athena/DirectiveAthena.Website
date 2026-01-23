// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using InfiniBlazor.Markdown.Parsers.Blazor;

namespace DirectiveAthena.Website.Services.Markdown;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public partial class InfiniMudMdTemplate(ITemplateDataProvider? templateContentProvider = null) {
    private ITemplateDataProvider? TemplateContentProvider { get; } = templateContentProvider;
}
