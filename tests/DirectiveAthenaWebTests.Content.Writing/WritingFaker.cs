// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using Bogus;
using DirectiveAthenaWeb.Content.Writing;
using DirectiveAthenaWeb.Services.Localization;

namespace DirectiveAthenaWebTests.Content.Writing;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public static class WritingFaker {
    public static WritingContent Create(int seed, bool hidden = false, bool includeNl = true) {
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

        return new WritingContent {
            Id = id,
            Title = title,
            Summary = summary,
            Tags = faker.Lorem.Words(2).ToList(),
            HiddenAt = hidden ? DateTime.UtcNow : DateTime.MinValue
        };
    }

    public static IReadOnlyCollection<LocalizationInfo> DefaultLocalizations()
        => [
            new LocalizationInfo("en", "English", "EN", "https://flagcdn.com/w40/us.png"),
            new LocalizationInfo("nl", "Nederlands", "NL", "https://flagcdn.com/w40/nl.png")
        ];
}
