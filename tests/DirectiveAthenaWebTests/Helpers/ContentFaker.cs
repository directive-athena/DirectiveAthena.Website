// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using Bogus;
using DirectiveAthenaWeb.Content.Faq;
using DirectiveAthenaWeb.Content.Note;

namespace DirectiveAthenaWebTests.Helpers;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public static class ContentFaker {
    public static NoteContent CreateNote(int seed, bool hidden = false, bool includeNl = true) {
        var faker = new Faker { Random = new Randomizer(seed) };
        Guid id = faker.Random.Guid();

        Dictionary<string, string> title = new() {
            ["en"] = faker.Lorem.Sentence(3)
        };
        Dictionary<string, string> summary = new() {
            ["en"] = faker.Lorem.Sentence(6)
        };

        if (includeNl) {
            title["nl"] = faker.Lorem.Sentence(3);
            summary["nl"] = faker.Lorem.Sentence(6);
        }

        return new NoteContent {
            Id = id,
            Title = title,
            Summary = summary,
            Tags = faker.Lorem.Words(2).ToList(),
            HiddenAt = hidden ? DateTime.UtcNow : DateTime.MinValue,
            InternalTitle = string.Empty
        };
    }

    public static FaqContent CreateFaq(int seed, bool includeNl = true) {
        var faker = new Faker { Random = new Randomizer(seed) };
        var faq = new FaqContent {
            Id = faker.Random.Guid(),
            Question = new Dictionary<string, string> {
                ["en"] = $"Question {seed}"
            },
            Answer = new Dictionary<string, string> {
                ["en"] = $"Answer {seed}"
            },
            InternalTitle = string.Empty
        };

        if (includeNl) {
            faq.Question["nl"] = $"Vraag {seed}";
            faq.Answer["nl"] = $"Antwoord {seed}";
        }

        return faq;
    }
}
