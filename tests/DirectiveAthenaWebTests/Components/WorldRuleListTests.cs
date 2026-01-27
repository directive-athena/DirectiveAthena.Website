// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using Bunit;
using DirectiveAthenaWeb.Components;
using DirectiveAthenaWeb.Services.Localization.Resources;
using DirectiveAthenaWeb.Services.WorldRules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using MudBlazor.Services;

namespace DirectiveAthenaWebTests.Components;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class WorldRuleListTests {
    [Test]
    public async Task WorldRuleList_RendersDeletedChip() {
        // Arrange
        await using var ctx = new BunitContext();
        ctx.Services.AddMudServices();
        ctx.Services.AddLocalization();
        ctx.JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);
        var localizer = ctx.Services.GetRequiredService<IStringLocalizer<Shared>>();

        var rule = new WorldRule {
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
        IRenderedComponent<WorldRuleList> component = ctx.Render<WorldRuleList>(parameters => parameters
            .Add(p => p.Rules, new List<WorldRule> { rule }));

        // Assert
        await Assert.That(component.Markup).Contains(localizer[Shared.ListDeleted]);
    }
}
