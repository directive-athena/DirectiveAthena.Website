// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using Bunit;
using DirectiveAthenaWeb.Components;
using DirectiveAthenaWeb.Services.Localization.Resources;
using DirectiveAthenaWeb.Services.WorldFaq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using MudBlazor.Services;

namespace DirectiveAthenaWebTests.Components;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class WorldFaqListTests {
    [Test]
    public async Task WorldFaqList_RendersDeletedChip() {
        // Arrange
        await using var ctx = new BunitContext();
        ctx.Services.AddMudServices();
        ctx.Services.AddLocalization();
        ctx.JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);
        var localizer = ctx.Services.GetRequiredService<IStringLocalizer<Shared>>();

        var rule = new WorldFaq {
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
        IRenderedComponent<WorldFaqList> component = ctx.Render<WorldFaqList>(parameters => parameters
            .Add(p => p.Rules, new List<WorldFaq> { rule }));

        // Assert
        await Assert.That(component.Markup).Contains(localizer[Shared.ListDeleted]);
    }
}
