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

    private static bool HasUniqueIds(IEnumerable<Article> articles) {
        HashSet<string> ids = new(StringComparer.Ordinal);
        foreach (Article article in articles) {
            if (!ids.Add(article.Id)) {
                return false;
            }
        }

        return true;
    }

    private static bool HasUniqueFiles(IEnumerable<Article> articles) {
        HashSet<string> files = new(StringComparer.OrdinalIgnoreCase);
        foreach (Article article in articles) {
            if (!files.Add(article.File)) {
                return false;
            }
        }

        return true;
    }
}
