// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthena.Website.Models;
using DirectiveAthena.Website.Services;
using DirectiveAthena.Website.Services.Validation;
using DirectiveAthenaTests.Website.Helpers;
using FluentValidation.Results;
using NSubstitute;

namespace DirectiveAthenaTests.Website.Services.Validation;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class ArticleCollectionValidatorTests {

    private static ILocalizationProvider CreateLocalizationProvider(string currentCode) {
        LocalizationInfo[] localizations = [
            new("en", "English", "EN", ""),
            new("nl", "Nederlands", "NL", "")
        ];

        var localizationProvider = Substitute.For<ILocalizationProvider>();
        localizationProvider.GetCurrentLocalization().Returns(localizations.First(l => l.Code == currentCode));
        localizationProvider.GetSupportedLocalizations().Returns(localizations);
        return localizationProvider;
    }

    // -----------------------------------------------------------------------------------------------------------------
    // Test Methods
    // -----------------------------------------------------------------------------------------------------------------
    [Test]
    public async Task Validate_RejectsDuplicateIds() {
        // Arrange
        Article[] articles = [
            ArticleFaker.Create(400),
            ArticleFaker.Create(401)
        ];
        articles[1].Id = articles[0].Id;

        var validator = new ArticleCollectionValidator(new ArticleValidator(CreateLocalizationProvider("en")));

        // Act
        ValidationResult? result = await validator.ValidateAsync(articles);

        // Assert
        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Errors.Any(e => e.ErrorMessage == "Duplicate IDs found!")).IsTrue();
    }

    [Test]
    public async Task Validate_RejectsDuplicateFiles_IgnoresCase() {
        // Arrange
        Article[] articles = [
            ArticleFaker.Create(410),
            ArticleFaker.Create(411)
        ];
        articles[1].File = articles[0].File.ToUpperInvariant();

        var validator = new ArticleCollectionValidator(new ArticleValidator(CreateLocalizationProvider("en")));

        // Act
        ValidationResult? result = await validator.ValidateAsync(articles);

        // Assert
        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Errors.Any(e => e.ErrorMessage == "Duplicate files found!")).IsTrue();
    }

    [Test]
    public async Task Validate_FailsWhenAnyArticleIsInvalid() {
        // Arrange
        Article valid = ArticleFaker.Create(420);
        Article invalid = ArticleFaker.Create(421, includeNl: false);
        Article[] articles = [valid, invalid];

        var validator = new ArticleCollectionValidator(new ArticleValidator(CreateLocalizationProvider("en")));

        // Act
        ValidationResult? result = await validator.ValidateAsync(articles);

        // Assert
        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Errors.Any(e => e.ErrorMessage == "Some posts have missing titles for one or more cultures!")).IsTrue();
    }
}
