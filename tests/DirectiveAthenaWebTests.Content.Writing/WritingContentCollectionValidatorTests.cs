// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Content.Note;
using DirectiveAthenaWeb.Content.Note.Services;
using DirectiveAthenaWeb.Services.Localization;
using FluentValidation.Results;
using NSubstitute;

namespace DirectiveAthenaWebTests.Content.Writing;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class NoteContentCollectionValidatorTests {

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
        NoteContent[] notes = [
            WritingFaker.Create(400),
            WritingFaker.Create(401)
        ];
        notes[1].Id = notes[0].Id;

        var validator = new NoteContentCollectionValidator(new NoteContentValidator(CreateLocalizationProvider("en")));

        // Act
        ValidationResult? result = await validator.ValidateAsync(notes);

        // Assert
        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Errors.Any(e => e.ErrorMessage == "Duplicate IDs found!")).IsTrue();
    }

    [Test]
    public async Task Validate_FailsWhenAnyWritingIsInvalid() {
        // Arrange
        NoteContent valid = WritingFaker.Create(420);
        NoteContent invalid = WritingFaker.Create(421, includeNl: false);
        NoteContent[] notes = [valid, invalid];

        var validator = new NoteContentCollectionValidator(new NoteContentValidator(CreateLocalizationProvider("en")));

        // Act
        ValidationResult? result = await validator.ValidateAsync(notes);

        // Assert
        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Errors.Any(e => e.ErrorMessage == "Some posts have missing titles for one or more cultures!")).IsTrue();
    }
}
