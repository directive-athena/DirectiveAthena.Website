// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using Bunit;
using DirectiveAthenaWeb.Content.Faq;
using DirectiveAthenaWeb.Content.Faq.Components;
using DirectiveAthenaWeb.Services.Localization;
using DirectiveAthenaWeb.Services.Localization.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
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
        ctx.Services.AddLogging();
        ctx.Services.AddSingleton(Options.Create(new DirectiveAthenaWeb.Services.Localization.LocalizationOptions()
            .AddLocalization("en", "English", "EN", "https://flagcdn.com/w40/us.png")
            .AddLocalization("nl", "Nederlands", "NL", "https://flagcdn.com/w40/nl.png")));
        ctx.Services.AddSingleton<ILocalizationProvider, LocalizationProvider>();
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
            SoftDeletedAt = DateTime.UtcNow,
            InternalTitle = string.Empty
        };

        // Act
        IRenderedComponent<FaqList> component = ctx.Render<FaqList>(parameters => parameters
            .Add(p => p.Rules, [rule]));

        // Assert
        await Assert.That(component.Markup).Contains(localizer[Shared.ListDeleted]);
    }
}
