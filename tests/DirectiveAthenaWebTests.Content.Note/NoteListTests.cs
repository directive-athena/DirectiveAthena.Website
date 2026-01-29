// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using Bunit;
using DirectiveAthenaWeb.Content.Note;
using DirectiveAthenaWeb.Services.Localization;
using DirectiveAthenaWeb.Services.Localization.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using MudBlazor.Services;

namespace DirectiveAthenaWebTests.Content.Note;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class NoteListTests {
    [Test]
    public async Task WritingList_RendersChipsForHiddenAndMissingNl() {
        // Arrange
        await using var ctx = new BunitContext();
        ctx.Services.AddMudServices();
        ctx.Services.AddLocalization();
        ctx.Services.AddLogging();
        ctx.Services.AddSingleton(Options.Create(new DirectiveAthenaWeb.Services.Localization.LocalizationOptions()
            .AddLocalization("en", "English", "EN", "https://flagcdn.com/w40/us.png")
            .AddLocalization("nl", "Nederlands", "NL", "https://flagcdn.com/w40/nl.png")));
        ctx.Services.AddSingleton<ILocalizationProvider, LocalizationProvider>();
        ctx.JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);
        var localizer = ctx.Services.GetRequiredService<IStringLocalizer<Shared>>();

        NoteContent post = NoteFaker.Create(200, hidden: true, includeNl: false);
        post.SoftDeletedAt = DateTime.UtcNow;
        List<NoteContent> posts = [post];

        // Act
        IRenderedComponent<NoteList> component = ctx.Render<NoteList>(parameters => parameters
            .Add(p => p.Notes, posts));

        // Assert
        await Assert.That(component.Markup).Contains(post.Title["en"]);
        await Assert.That(component.Markup).Contains(localizer[Shared.ListHidden]);
        await Assert.That(component.Markup).Contains(localizer[Shared.ListDeleted]);
        await Assert.That(component.Markup).Contains(localizer[Shared.ListMissingNl]);
    }
}
