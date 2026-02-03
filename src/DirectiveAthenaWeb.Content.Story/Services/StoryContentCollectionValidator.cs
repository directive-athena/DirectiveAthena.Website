// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using FluentValidation;

namespace DirectiveAthenaWeb.Content.Story.Services;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableTransient<IValidator<IEnumerable<StoryContent>>>]
internal class StoryContentCollectionValidator : AbstractValidator<IEnumerable<StoryContent>> {
    public StoryContentCollectionValidator(IValidator<StoryContent> articleValidator) {
        RuleFor(stories => stories)
            .NotNull();

        RuleForEach(stories => stories)
            .SetValidator(articleValidator);

        RuleFor(stories => stories)
            .Must(HasUniqueIds)
            .WithMessage("Duplicate IDs found!");
    }

    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    private static bool HasUniqueIds(IEnumerable<StoryContent> stories) {
        HashSet<Guid> ids = [];
        return stories.All(article => ids.Add(article.Id));
    }
}
