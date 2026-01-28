// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using Bunit;
using DirectiveAthenaWeb.Components;
using DirectiveAthenaWeb.Content.Note;
using DirectiveAthenaWeb.Services.Localization.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using MudBlazor.Services;

namespace DirectiveAthenaWebTests.Content.Writing;
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
        ctx.JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);
        var localizer = ctx.Services.GetRequiredService<IStringLocalizer<Shared>>();

        NoteContent post = WritingFaker.Create(200, hidden: true, includeNl: false);
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
