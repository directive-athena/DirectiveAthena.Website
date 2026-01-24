// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using Bunit;
using DirectiveAthena.Website.Components;
using DirectiveAthena.Website.Resources;
using DirectiveAthena.Website.Services.Articles;
using DirectiveAthenaTests.Website.Helpers;
using MudBlazor.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace DirectiveAthenaTests.Website.Components;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class ArticleListTests {
    [Test]
    public async Task ArticleList_RendersChipsForHiddenAndMissingNl() {
        // Arrange
        await using var ctx = new BunitContext();
        ctx.Services.AddMudServices();
        ctx.Services.AddLocalization();
        ctx.JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);
        IStringLocalizer<Shared> localizer = ctx.Services.GetRequiredService<IStringLocalizer<Shared>>();

        Article post = ArticleFaker.Create(200, hidden: true, includeNl: false);
        List<Article> posts = [post];

        // Act
        IRenderedComponent<ArticleList> component = ctx.Render<ArticleList>(parameters => parameters
            .Add(p => p.Posts, posts));

        // Assert
        await Assert.That(component.Markup).Contains(post.Title["en"]);
        await Assert.That(component.Markup).Contains(localizer[Shared.ListHidden]);
        await Assert.That(component.Markup).Contains(localizer[Shared.ListMissingNl]);
    }
}
