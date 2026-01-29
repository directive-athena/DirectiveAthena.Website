// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using Bunit;
using DirectiveAthenaWeb.Content.Faq;
using DirectiveAthenaWeb.Content.Faq.Components;
using DirectiveAthenaWeb.Services.Localization.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using MudBlazor.Services;

namespace DirectiveAthenaWebTests.Content.Faq;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class FaqListTests {
    [Test]
    public async Task FaqList_RendersDeletedChip() {
        // Arrange
        await using var ctx = new BunitContext();
        ctx.Services.AddMudServices();
        ctx.Services.AddLocalization();
        ctx.JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);
        var localizer = ctx.Services.GetRequiredService<IStringLocalizer<Shared>>();

        var rule = new FaqContent {
            Id = Guid.NewGuid(),
            Question = new Dictionary<string, string> {
                ["en"] = "Question",
                ["nl"] = "Vraag"
            },
            Answer = new Dictionary<string, string> {
                ["en"] = "Answer",
                ["nl"] = "Antwoord"
            },
            SoftDeletedAt = DateTime.UtcNow
        };

        // Act
        IRenderedComponent<FaqList> component = ctx.Render<FaqList>(parameters => parameters
            .Add(p => p.Rules, [rule]));

        // Assert
        await Assert.That(component.Markup).Contains(localizer[Shared.ListDeleted]);
    }
}
