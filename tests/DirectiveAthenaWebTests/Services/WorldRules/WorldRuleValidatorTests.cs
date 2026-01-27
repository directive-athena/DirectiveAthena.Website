// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Content.Faq.Services;
using DirectiveAthenaWeb.Services.Localization;
using DirectiveAthenaWeb.Services.WorldFaq;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;

namespace DirectiveAthenaWebTests.Services.WorldRules;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class WorldFaqValidatorTests {
    private static ILocalizationProvider CreateLocalizationProvider() {
        LocalizationInfo[] localizations = [
            new("en", "English", "EN", ""),
            new("nl", "Nederlands", "NL", "")
        ];

        var provider = Substitute.For<ILocalizationProvider>();
        provider.GetSupportedLocalizations().Returns(localizations);
        provider.DefaultLocalization.Returns(localizations.First(l => l.Code == "en"));
        return provider;
    }

    private static WorldFaq CreateValidRule() => new() {
        Id = Guid.NewGuid(),
        Date = "2026-01-01",
        Question = new Dictionary<string, string> {
            ["en"] = "Question",
            ["nl"] = "Vraag"
        },
        Answer = new Dictionary<string, string> {
            ["en"] = "Answer",
            ["nl"] = "Antwoord"
        }
    };

    [Test]
    public async Task WorldFaqValidator_RejectsMissingId() {
        // Arrange
        WorldFaq rule = CreateValidRule();
        rule.Id = Guid.Empty;
        var validator = new WorldFaqValidator(CreateLocalizationProvider());

        // Act
        ValidationResult? result = await validator.ValidateAsync(rule);

        // Assert
        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Errors.First().ErrorMessage).IsEqualTo("Some rules have missing Id!");
    }

    [Test]
    public async Task WorldFaqValidator_RejectsMissingDate() {
        // Arrange
        WorldFaq rule = CreateValidRule();
        rule.Date = "";
        var validator = new WorldFaqValidator(CreateLocalizationProvider());

        // Act
        ValidationResult? result = await validator.ValidateAsync(rule);

        // Assert
        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Errors.First().ErrorMessage).IsEqualTo("Some rules have missing dates!");
    }

    [Test]
    public async Task WorldFaqValidator_RejectsMissingLocalizedQuestion() {
        // Arrange
        WorldFaq rule = CreateValidRule();
        rule.Question.Remove("nl");
        var validator = new WorldFaqValidator(CreateLocalizationProvider());

        // Act
        ValidationResult? result = await validator.ValidateAsync(rule);

        // Assert
        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Errors.First().ErrorMessage)
            .IsEqualTo("Some rules have missing questions for one or more cultures!");
    }

    [Test]
    public async Task WorldFaqValidator_RejectsMissingLocalizedAnswer() {
        // Arrange
        WorldFaq rule = CreateValidRule();
        rule.Answer.Remove("nl");
        var validator = new WorldFaqValidator(CreateLocalizationProvider());

        // Act
        ValidationResult? result = await validator.ValidateAsync(rule);

        // Assert
        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Errors.First().ErrorMessage)
            .IsEqualTo("Some rules have missing answers for one or more cultures!");
    }

    [Test]
    public async Task WorldFaqCollectionValidator_RejectsDuplicateIds() {
        // Arrange
        var sharedId = Guid.NewGuid();
        WorldFaq[] rules = [
            CreateValidRule(),
            CreateValidRule()
        ];
        rules[0].Id = sharedId;
        rules[1].Id = sharedId;

        IValidator<WorldFaq> ruleValidator = new WorldFaqValidator(CreateLocalizationProvider());
        var collectionValidator = new WorldFaqCollectionValidator(ruleValidator);

        // Act
        ValidationResult? result = await collectionValidator.ValidateAsync(rules);

        // Assert
        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Errors.First().ErrorMessage).IsEqualTo("Duplicate IDs found!");
    }
}
