// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Content.Note;
using DirectiveAthenaWeb.Content.Note.Services;
using FluentValidation.Results;
using DirectiveAthenaWebTests.Helpers;

namespace DirectiveAthenaWebTests.Content.Note;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class NoteContentCollectionValidatorTests {
    // -----------------------------------------------------------------------------------------------------------------
    // Test Methods
    // -----------------------------------------------------------------------------------------------------------------
    [Test]
    public async Task Validate_RejectsDuplicateIds() {
        // Arrange
        NoteContent[] notes = [
            ContentFaker.CreateNote(400),
            ContentFaker.CreateNote(401)
        ];
        notes[1].Id = notes[0].Id;

        var validator = new NoteContentCollectionValidator(new NoteContentValidator(TestLocalization.CreateLocalizationProvider("en")));

        // Act
        ValidationResult? result = await validator.ValidateAsync(notes);

        // Assert
        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Errors.Any(e => e.ErrorMessage == "Duplicate IDs found!")).IsTrue();
    }

    [Test]
    public async Task Validate_FailsWhenAnyWritingIsInvalid() {
        // Arrange
        NoteContent valid = ContentFaker.CreateNote(420);
        NoteContent invalid = ContentFaker.CreateNote(421, includeNl: false);
        NoteContent[] notes = [valid, invalid];

        var validator = new NoteContentCollectionValidator(new NoteContentValidator(TestLocalization.CreateLocalizationProvider("en")));

        // Act
        ValidationResult? result = await validator.ValidateAsync(notes);

        // Assert
        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Errors.Any(e => e.ErrorMessage == "Some posts have missing titles for one or more cultures!")).IsTrue();
    }
}
