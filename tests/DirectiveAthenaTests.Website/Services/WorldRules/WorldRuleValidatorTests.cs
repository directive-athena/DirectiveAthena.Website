// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthena.Website.Services.Localization;
using DirectiveAthena.Website.Services.WorldRules;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;

namespace DirectiveAthenaTests.Website.Services.WorldRules;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class WorldRuleValidatorTests {
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

    private static WorldRule CreateValidRule() => new() {
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
    public async Task WorldRuleValidator_RejectsMissingId() {
        // Arrange
        WorldRule rule = CreateValidRule();
        rule.Id = Guid.Empty;
        var validator = new WorldRuleValidator(CreateLocalizationProvider());

        // Act
        ValidationResult? result = validator.Validate(rule);

        // Assert
        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Errors.First().ErrorMessage).IsEqualTo("Some rules have missing Id!");
    }

    [Test]
    public async Task WorldRuleValidator_RejectsMissingDate() {
        // Arrange
        WorldRule rule = CreateValidRule();
        rule.Date = "";
        var validator = new WorldRuleValidator(CreateLocalizationProvider());

        // Act
        ValidationResult? result = validator.Validate(rule);

        // Assert
        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Errors.First().ErrorMessage).IsEqualTo("Some rules have missing dates!");
    }

    [Test]
    public async Task WorldRuleValidator_RejectsMissingLocalizedQuestion() {
        // Arrange
        WorldRule rule = CreateValidRule();
        rule.Question.Remove("nl");
        var validator = new WorldRuleValidator(CreateLocalizationProvider());

        // Act
        ValidationResult? result = validator.Validate(rule);

        // Assert
        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Errors.First().ErrorMessage)
            .IsEqualTo("Some rules have missing questions for one or more cultures!");
    }

    [Test]
    public async Task WorldRuleValidator_RejectsMissingLocalizedAnswer() {
        // Arrange
        WorldRule rule = CreateValidRule();
        rule.Answer.Remove("nl");
        var validator = new WorldRuleValidator(CreateLocalizationProvider());

        // Act
        ValidationResult? result = validator.Validate(rule);

        // Assert
        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Errors.First().ErrorMessage)
            .IsEqualTo("Some rules have missing answers for one or more cultures!");
    }

    [Test]
    public async Task WorldRuleCollectionValidator_RejectsDuplicateIds() {
        // Arrange
        Guid sharedId = Guid.NewGuid();
        WorldRule[] rules = [
            CreateValidRule(),
            CreateValidRule()
        ];
        rules[0].Id = sharedId;
        rules[1].Id = sharedId;

        IValidator<WorldRule> ruleValidator = new WorldRuleValidator(CreateLocalizationProvider());
        var collectionValidator = new WorldRuleCollectionValidator(ruleValidator);

        // Act
        ValidationResult? result = collectionValidator.Validate(rules);

        // Assert
        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Errors.First().ErrorMessage).IsEqualTo("Duplicate IDs found!");
    }
}
