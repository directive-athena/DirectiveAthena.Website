// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthenaWeb.Services.WorldFaq;
using FluentValidation;

namespace DirectiveAthenaWeb.Content.Faq.Services;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableTransient<IValidator<IEnumerable<WorldFaq>>>]
public class WorldFaqCollectionValidator : AbstractValidator<IEnumerable<WorldFaq>> {
    public WorldFaqCollectionValidator(IValidator<WorldFaq> ruleValidator) {
        RuleFor(rules => rules)
            .NotNull();

        RuleForEach(rules => rules)
            .SetValidator(ruleValidator);

        RuleFor(rules => rules)
            .Must(HasUniqueIds)
            .WithMessage("Duplicate IDs found!");
    }

    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    private static bool HasUniqueIds(IEnumerable<WorldFaq> rules) {
        HashSet<Guid> ids = [];
        return rules.All(rule => ids.Add(rule.Id));
    }

}
