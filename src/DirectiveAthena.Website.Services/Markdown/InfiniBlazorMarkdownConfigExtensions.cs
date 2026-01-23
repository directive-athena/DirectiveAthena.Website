// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using InfiniBlazor.Markdown;
using InfiniBlazor.Markdown.Syntax.Nodes;

namespace DirectiveAthena.Website.Services.Markdown;

// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public static class InfiniBlazorMarkdownConfigExtensions {
    public static InfiniBlazorMarkdownConfig WithMudBlazorComponents(this InfiniBlazorMarkdownConfig config) {
        config.HtmlRendererFootnoteWrapperType = typeof(InfiniMudMdEditorFootnoteDescriptionWrapper);
        
        config.RegisterMdBlazorComponent<BlockQuoteMdSyntaxNode, InfiniMudMdBlockQuote>();
        config.RegisterMdBlazorComponent<BoldMdSyntaxNode, InfiniMudMdBold>();
        config.RegisterMdBlazorComponent<CalloutMdSyntaxNode, InfiniMudMdCallout>();
        config.RegisterMdBlazorComponent<CodeBlockMdSyntaxNode, InfiniMudMdCodeBlock>();
        config.RegisterMdBlazorComponent<CodeInlineMdSyntaxNode, InfiniMudMdCodeInline>();
        config.RegisterMdBlazorComponent<HtmlMdSyntaxNode, InfiniMudMdHtml>();
        config.RegisterMdBlazorComponent<TextMdSyntaxNode, InfiniMudMdText>();
        config.RegisterMdBlazorComponent<EmoteMdSyntaxNode, InfiniMudMdEmote>();
        config.RegisterMdBlazorComponent<EscapedCharacterMdSyntaxNode, InfiniMudMdEscapedCharacter>();
        config.RegisterMdBlazorComponent<HeadingMdSyntaxNode, InfiniMudMdHeading>();
        config.RegisterMdBlazorComponent<HeadingSimpleMdSyntaxNode, InfiniMudMdHeadingSimple>();
        config.RegisterMdBlazorComponent<HorizontalRuleMdSyntaxNode, InfiniMudMdHorizontalRule>();
        config.RegisterMdBlazorComponent<HtmlSpanMdSyntaxNode, InfiniMudMdHtmlSpan>();
        config.RegisterMdBlazorComponent<ImageMdSyntaxNode, InfiniMudMdImage>();
        config.RegisterMdBlazorComponent<ItalicMdSyntaxNode, InfiniMudMdItalic>();
        config.RegisterMdBlazorComponent<LinkMdSyntaxNode, InfiniMudMdLink>();
        config.RegisterMdBlazorComponent<ListItemMdSyntaxNode, InfiniMudMdListItem>();
        config.RegisterMdBlazorComponent<ListOrderedMdSyntaxNode, InfiniMudMdListOrdered>();
        config.RegisterMdBlazorComponent<ListUnOrderedMdSyntaxNode, InfiniMudMdListUnOrdered>();
        config.RegisterMdBlazorComponent<ParagraphMdSyntaxNode, InfiniMudMdParagraph>();
        config.RegisterMdBlazorComponent<StrikeMdSyntaxNode, InfiniMudMdStrike>();
        config.RegisterMdBlazorComponent<SubScriptMdSyntaxNode, InfiniMudMdSubScript>();
        config.RegisterMdBlazorComponent<SuperScriptMdSyntaxNode, InfiniMudMdSuperScript>();
        config.RegisterMdBlazorComponent<TableMdSyntaxNode, InfiniMudMdTable>();
        config.RegisterMdBlazorComponent<TagMdSyntaxNode, InfiniMudMdTag>();
        config.RegisterMdBlazorComponent<UnderlineMdSyntaxNode, InfiniMudMdUnderline>();
        config.RegisterMdBlazorComponent<UserMdSyntaxNode, InfiniMudMdUser>();
        config.RegisterMdBlazorComponent<WikiLinkMdSyntaxNode, InfiniMudMdWikiLink>();
        config.RegisterMdBlazorComponent<TemplateMdSyntaxNode, InfiniMudMdTemplate>();
        config.RegisterMdBlazorComponent<FootnoteReferenceMdSyntaxNode, InfiniMudMdFootnoteReference>();
        config.RegisterMdBlazorComponent<FootnoteDescriptionMdSyntaxNode, InfiniMudMdFootnoteDescription>();
        config.RegisterMdBlazorComponent<HighlightMdSyntaxNode, InfiniMudMdHighlight>();
        config.RegisterMdBlazorComponent<WrapperMdSyntaxNode, InfiniMudMdWrapper>();
        config.RegisterMdBlazorComponent<FrontMatterMdSyntaxNode, InfiniMudMdFrontMatter>();
        config.RegisterMdBlazorComponent<BreakMdSyntaxNode, InfiniMudMdBreak>();
        // config.RegisterBlazorComponent<NewLineMdSyntaxNode, InfiniMudMdNewLine>(); // Not implemented well yet, only as an example
        
        return config;
    }
}
