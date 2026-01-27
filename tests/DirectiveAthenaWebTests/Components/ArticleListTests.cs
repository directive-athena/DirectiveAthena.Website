// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using Bunit;
using DirectiveAthenaWeb.Components;
using DirectiveAthenaWeb.Services.Articles;
using DirectiveAthenaWeb.Services.Localization.Resources;
using DirectiveAthenaWebTests.Helpers;
using MudBlazor.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace DirectiveAthenaWebTests.Components;
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
