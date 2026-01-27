// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using Bunit;
using DirectiveAthenaWeb.Components;
using DirectiveAthenaWeb.Content.Writings;
using DirectiveAthenaWeb.Services.Localization.Resources;
using DirectiveAthenaWebTests.Helpers;
using MudBlazor.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace DirectiveAthenaWebTests.Components;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class WritingListTests {
    [Test]
    public async Task WritingList_RendersChipsForHiddenAndMissingNl() {
        // Arrange
        await using var ctx = new BunitContext();
        ctx.Services.AddMudServices();
        ctx.Services.AddLocalization();
        ctx.JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);
        var localizer = ctx.Services.GetRequiredService<IStringLocalizer<Shared>>();

        WritingContent post = WritingFaker.Create(200, hidden: true, includeNl: false);
        post.SoftDeletedAt = DateTime.UtcNow;
        List<WritingContent> posts = [post];

        // Act
        IRenderedComponent<WritingList> component = ctx.Render<WritingList>(parameters => parameters
            .Add(p => p.Posts, posts));

        // Assert
        await Assert.That(component.Markup).Contains(post.Title["en"]);
        await Assert.That(component.Markup).Contains(localizer[Shared.ListHidden]);
        await Assert.That(component.Markup).Contains(localizer[Shared.ListDeleted]);
        await Assert.That(component.Markup).Contains(localizer[Shared.ListMissingNl]);
    }
}
