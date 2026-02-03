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
public class NoteContentValidatorTests {
    // -----------------------------------------------------------------------------------------------------------------
    // Test Methods
    // -----------------------------------------------------------------------------------------------------------------
    [Test]
    public async Task Validate_RejectsMissingId() {
        // Arrange
        var validator = new NoteContentValidator(TestLocalization.CreateLocalizationProvider());
        NoteContent article = ContentFaker.CreateNote(300);
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
        var validator = new NoteContentValidator(TestLocalization.CreateLocalizationProvider());
        NoteContent article = ContentFaker.CreateNote(301, includeNl: false);

        // Act
        ValidationResult? result = await validator.ValidateAsync(article);

        // Assert
        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Errors.Any(e => e.ErrorMessage == "Some posts have missing titles for one or more cultures!")).IsTrue();
    }

    [Test]
    public async Task Validate_RejectsMissingLocalizedSummary() {
        // Arrange
        var validator = new NoteContentValidator(TestLocalization.CreateLocalizationProvider());
        NoteContent article = ContentFaker.CreateNote(302);
        article.Summary.Remove("nl");

        // Act
        ValidationResult? result = await validator.ValidateAsync(article);

        // Assert
        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Errors.Any(e => e.ErrorMessage == "Some posts have missing summaries for one or more cultures!")).IsTrue();
    }

    [Test]
    public async Task Validate_AllowsValidWriting() {
        // Arrange
        var validator = new NoteContentValidator(TestLocalization.CreateLocalizationProvider());
        NoteContent article = ContentFaker.CreateNote(303);

        // Act
        ValidationResult? result = await validator.ValidateAsync(article);

        // Assert
        await Assert.That(result.IsValid).IsTrue();
    }
}
