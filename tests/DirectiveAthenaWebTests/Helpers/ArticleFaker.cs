// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using Bogus;
using DirectiveAthenaWeb.Services.Articles;
using DirectiveAthenaWeb.Services.Localization;

namespace DirectiveAthenaWebTests.Helpers;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public static class ArticleFaker {
    public static Article Create(int seed, bool hidden = false, bool includeNl = true) {
        var faker = new Faker {
            Random = new Randomizer(seed)
        };
        Guid id = faker.Random.Guid();

        Dictionary<string, string> title = new() {
            ["en"] = faker.Lorem.Sentence(3)
        };
        Dictionary<string, string> summary = new() {
            ["en"] = faker.Lorem.Sentence(6)
        };

        // ReSharper disable once InvertIf
        if (includeNl) {
            title["nl"] = faker.Lorem.Sentence(3);
            summary["nl"] = faker.Lorem.Sentence(6);
        }

        return new Article {
            Id = id,
            Title = title,
            Summary = summary,
            Tags = faker.Lorem.Words(2).ToList(),
            IsHidden = hidden
        };
    }

    public static IReadOnlyCollection<LocalizationInfo> DefaultLocalizations()
        => LocalizationProvider.SupportedLocalizations;
}
