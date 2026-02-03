// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using Bunit;
using DirectiveAthenaWeb.Content.Note;
using DirectiveAthenaWeb.Services.Localization;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using DirectiveAthenaWebTests.Helpers;

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
        ctx.Services.AddSingleton(TestLocalization.CreateLocalizationOptions());
        ctx.Services.AddSingleton<ILocalizationProvider, LocalizationProvider>();
        ctx.JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);

        NoteContent post = ContentFaker.CreateNote(200, hidden: true, includeNl: false);
        post.SoftDeletedAt = DateTime.UtcNow;
        List<NoteContent> posts = [post];

        // Act
        IRenderedComponent<NoteList> component = ctx.Render<NoteList>(parameters => parameters
            .Add(p => p.Notes, posts));

        // Assert
        await Assert.That(component.Markup).IsEmpty();
    }
}
