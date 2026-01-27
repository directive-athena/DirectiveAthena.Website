// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Services.Articles;
using DirectiveAthenaWeb.Services.Localization;
using DirectiveAthenaWebTests.Helpers;
using FluentValidation.Results;
using NSubstitute;

namespace DirectiveAthenaWebTests.Services.Articles;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class ArticleValidatorTests {

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
    public async Task Validate_RejectsMissingId() {
        // Arrange
        var validator = new ArticleValidator(CreateLocalizationProvider("en"));
        Article article = ArticleFaker.Create(300);
        article.Id = Guid.Empty;

        // Act
        ValidationResult? result = await validator.ValidateAsync(article);

        // Assert
        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Errors.Any(e => e.ErrorMessage == "Some posts have missing Id!")).IsTrue();
    }

    [Test]
    public async Task Validate_RejectsMissingLocalizedTitle() {
        // Arrange
        var validator = new ArticleValidator(CreateLocalizationProvider("en"));
        Article article = ArticleFaker.Create(301, includeNl: false);

        // Act
        ValidationResult? result = await validator.ValidateAsync(article);

        // Assert
        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Errors.Any(e => e.ErrorMessage == "Some posts have missing titles for one or more cultures!")).IsTrue();
    }

    [Test]
    public async Task Validate_RejectsMissingLocalizedSummary() {
        // Arrange
        var validator = new ArticleValidator(CreateLocalizationProvider("en"));
        Article article = ArticleFaker.Create(302);
        article.Summary.Remove("nl");

        // Act
        ValidationResult? result = await validator.ValidateAsync(article);

        // Assert
        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Errors.Any(e => e.ErrorMessage == "Some posts have missing summaries for one or more cultures!")).IsTrue();
    }

    [Test]
    public async Task Validate_AllowsValidArticle() {
        // Arrange
        var validator = new ArticleValidator(CreateLocalizationProvider("en"));
        Article article = ArticleFaker.Create(303);

        // Act
        ValidationResult? result = await validator.ValidateAsync(article);

        // Assert
        await Assert.That(result.IsValid).IsTrue();
    }
}
