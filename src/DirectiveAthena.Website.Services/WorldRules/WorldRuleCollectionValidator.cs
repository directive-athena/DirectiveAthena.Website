// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using FluentValidation;

namespace DirectiveAthena.Website.Services.WorldRules;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableTransient<IValidator<IEnumerable<WorldRule>>>]
public sealed class WorldRuleCollectionValidator : AbstractValidator<IEnumerable<WorldRule>> {
    public WorldRuleCollectionValidator(IValidator<WorldRule> ruleValidator) {
        RuleFor(rules => rules)
            .NotNull();

        RuleForEach(rules => rules)
            .SetValidator(ruleValidator);

        RuleFor(rules => rules)
            .Must(HasUniqueIds)
            .WithMessage("Duplicate IDs found!");

        RuleFor(rules => rules)
            .Must(HasUniqueFiles)
            .WithMessage("Duplicate files found!");
    }

    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    private static bool HasUniqueIds(IEnumerable<WorldRule> rules) {
        HashSet<string> ids = new(StringComparer.Ordinal);
        return rules.All(rule => ids.Add(rule.Id));
    }

    private static bool HasUniqueFiles(IEnumerable<WorldRule> rules) {
        HashSet<string> files = new(StringComparer.OrdinalIgnoreCase);
        return rules.All(rule => files.Add(rule.File));
    }
}
