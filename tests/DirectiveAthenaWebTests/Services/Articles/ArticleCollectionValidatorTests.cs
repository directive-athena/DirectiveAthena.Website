// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Content.Writings;
using DirectiveAthenaWeb.Content.Writings.Services;
using DirectiveAthenaWeb.Services.Localization;
using DirectiveAthenaWebTests.Helpers;
using FluentValidation.Results;
using NSubstitute;

namespace DirectiveAthenaWebTests.Services.Articles;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class WritingCollectionValidatorTests {

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
        WritingContent[] writings = [
            WritingFaker.Create(400),
            WritingFaker.Create(401)
        ];
        writings[1].Id = writings[0].Id;

        var validator = new WritingContentCollectionValidator(new WritingContentValidator(CreateLocalizationProvider("en")));

        // Act
        ValidationResult? result = await validator.ValidateAsync(writings);

        // Assert
        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Errors.Any(e => e.ErrorMessage == "Duplicate IDs found!")).IsTrue();
    }

    [Test]
    public async Task Validate_FailsWhenAnyWritingIsInvalid() {
        // Arrange
        WritingContent valid = WritingFaker.Create(420);
        WritingContent invalid = WritingFaker.Create(421, includeNl: false);
        WritingContent[] writings = [valid, invalid];

        var validator = new WritingContentCollectionValidator(new WritingContentValidator(CreateLocalizationProvider("en")));

        // Act
        ValidationResult? result = await validator.ValidateAsync(writings);

        // Assert
        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Errors.Any(e => e.ErrorMessage == "Some posts have missing titles for one or more cultures!")).IsTrue();
    }
}
