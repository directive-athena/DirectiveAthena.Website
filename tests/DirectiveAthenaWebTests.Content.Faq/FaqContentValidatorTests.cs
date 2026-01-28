// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Content.Faq;
using DirectiveAthenaWeb.Content.Faq.Services;
using DirectiveAthenaWeb.Services.Localization;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;

namespace DirectiveAthenaWebTests.Content.Faq;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class FaqContentValidatorTests {
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

    private static FaqContent CreateValidRule() => new() {
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
    public async Task FaqValidator_RejectsMissingId() {
        // Arrange
        FaqContent rule = CreateValidRule();
        rule.Id = Guid.Empty;
        var validator = new FaqContentValidator(CreateLocalizationProvider());

        // Act
        ValidationResult? result = await validator.ValidateAsync(rule);

        // Assert
        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Errors.First().ErrorMessage).IsEqualTo("Some rules have missing Id!");
    }

    [Test]
    public async Task FaqValidator_RejectsMissingDate() {
        // Arrange
        FaqContent rule = CreateValidRule();
        rule.Date = "";
        var validator = new FaqContentValidator(CreateLocalizationProvider());

        // Act
        ValidationResult? result = await validator.ValidateAsync(rule);

        // Assert
        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Errors.First().ErrorMessage).IsEqualTo("Some rules have missing dates!");
    }

    [Test]
    public async Task FaqValidator_RejectsMissingLocalizedQuestion() {
        // Arrange
        FaqContent rule = CreateValidRule();
        rule.Question.Remove("nl");
        var validator = new FaqContentValidator(CreateLocalizationProvider());

        // Act
        ValidationResult? result = await validator.ValidateAsync(rule);

        // Assert
        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Errors.First().ErrorMessage)
            .IsEqualTo("Some rules have missing questions for one or more cultures!");
    }

    [Test]
    public async Task FaqValidator_RejectsMissingLocalizedAnswer() {
        // Arrange
        FaqContent rule = CreateValidRule();
        rule.Answer.Remove("nl");
        var validator = new FaqContentValidator(CreateLocalizationProvider());

        // Act
        ValidationResult? result = await validator.ValidateAsync(rule);

        // Assert
        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Errors.First().ErrorMessage)
            .IsEqualTo("Some rules have missing answers for one or more cultures!");
    }

    [Test]
    public async Task FaqCollectionValidator_RejectsDuplicateIds() {
        // Arrange
        var sharedId = Guid.NewGuid();
        FaqContent[] rules = [
            CreateValidRule(),
            CreateValidRule()
        ];
        rules[0].Id = sharedId;
        rules[1].Id = sharedId;

        IValidator<FaqContent> ruleValidator = new FaqContentValidator(CreateLocalizationProvider());
        var collectionValidator = new FaqContentCollectionValidator(ruleValidator);

        // Act
        ValidationResult? result = await collectionValidator.ValidateAsync(rules);

        // Assert
        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Errors.First().ErrorMessage).IsEqualTo("Duplicate IDs found!");
    }
}
