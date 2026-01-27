// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using FluentValidation;

namespace DirectiveAthenaWeb.Services.WorldRules;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableTransient<IValidator<IEnumerable<WorldRule>>>]
public class WorldRuleCollectionValidator : AbstractValidator<IEnumerable<WorldRule>> {
    public WorldRuleCollectionValidator(IValidator<WorldRule> ruleValidator) {
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
    private static bool HasUniqueIds(IEnumerable<WorldRule> rules) {
        HashSet<Guid> ids = [];
        return rules.All(rule => ids.Add(rule.Id));
    }

}
