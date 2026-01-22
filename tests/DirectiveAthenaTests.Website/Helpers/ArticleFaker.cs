// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using Bogus;
using DirectiveAthena.Website.Models;
using DirectiveAthena.Website.Services;

namespace DirectiveAthenaTests.Website.Helpers;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public static class ArticleFaker {
    public static Article Create(int seed, bool hidden = false, bool includeNl = true) {
        var faker = new Faker {
            Random = new Randomizer(seed)
        };
        string id = faker.Random.Guid().ToString("N");
        string file = $"{id}.md";

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
            Date = faker.Date.Recent(30).ToString("yyyy-MM-dd"),
            Tags = faker.Lorem.Words(2).ToList(),
            File = file,
            Hidden = hidden
        };
    }

    public static IReadOnlyCollection<LocalizationInfo> DefaultLocalizations()
        => LocalizationProvider.SupportedLocalizations;
}
