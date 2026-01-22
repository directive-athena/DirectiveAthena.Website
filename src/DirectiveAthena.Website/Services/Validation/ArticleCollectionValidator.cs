// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using FluentValidation;
using DirectiveAthena.Website.Models;

namespace DirectiveAthena.Website.Services.Validation;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public sealed class ArticleCollectionValidator : AbstractValidator<IEnumerable<Article>> {
    public ArticleCollectionValidator(IValidator<Article> articleValidator) {
        RuleFor(articles => articles)
            .NotNull();

        RuleForEach(articles => articles)
            .SetValidator(articleValidator);

        RuleFor(articles => articles)
            .Must(HasUniqueIds)
            .WithMessage("Duplicate IDs found!");

        RuleFor(articles => articles)
            .Must(HasUniqueFiles)
            .WithMessage("Duplicate files found!");
    }

    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    private static bool HasUniqueIds(IEnumerable<Article> articles) {
        HashSet<string> ids = new(StringComparer.Ordinal);
        return articles.All(article => ids.Add(article.Id));

    }

    private static bool HasUniqueFiles(IEnumerable<Article> articles) {
        HashSet<string> files = new(StringComparer.OrdinalIgnoreCase);
        return articles.All(article => files.Add(article.File));

    }
}
