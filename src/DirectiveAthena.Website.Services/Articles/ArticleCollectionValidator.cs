// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using FluentValidation;

namespace DirectiveAthena.Website.Services.Articles;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableTransient<IValidator<IEnumerable<Article>>>]
public sealed class ArticleCollectionValidator : AbstractValidator<IEnumerable<Article>> {
    public ArticleCollectionValidator(IValidator<Article> articleValidator) {
        RuleFor(articles => articles)
            .NotNull();

        RuleForEach(articles => articles)
            .SetValidator(articleValidator);

        RuleFor(articles => articles)
            .Must(HasUniqueIds)
            .WithMessage("Duplicate IDs found!");
    }

    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    private static bool HasUniqueIds(IEnumerable<Article> articles) {
        HashSet<Guid> ids = [];
        return articles.All(article => ids.Add(article.Id));
    }
}
