// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthenaWeb.Services.Content;
using DirectiveAthenaWeb.Services.Localization;
using Microsoft.Extensions.Logging;

namespace DirectiveAthenaWeb.Content.Story.Services;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableScoped<IStoryContentManager>]
[InjectableScoped<IContentManager<StoryContent>>]
internal class StoryContentManager(
    ILocalizationProvider localizationProvider,
    IServiceProvider provider
) : ContentManagerBase<StoryContent>(provider), IStoryContentManager {

    public string GetLocalizedTitle(StoryContent article)
        => GetLocalizedValue(article.Title);

    public string GetLocalizedSummary(StoryContent article)
        => GetLocalizedValue(article.Summary);

    private string GetLocalizedValue(Dictionary<string, string> values) {
        LocalizationInfo localization = localizationProvider.GetCurrentLocalization();
        return !values.TryGetValue(localization.Code, out string? value)
            ? values.GetValueOrDefault(localizationProvider.DefaultLocalization.Code, string.Empty)
            : value;
    }

    public string GetLocalizedFilePath(StoryContent article) {
        LocalizationInfo localization = localizationProvider.GetCurrentLocalization();
        return Storage.GetMarkdownContentPath(localization.Code, article.MarkdownFileName);
    }

    public async Task<string> GetRawMarkdownContentAsync(StoryContent article, string locale, CancellationToken ct = default) {
        try {
            string path = Storage.GetMarkdownDiskPath(locale, article.MarkdownFileName);
            Logger.Debug("Fetching markdown for article {Id} at {Path}.", article.Id, path);
            return await Storage.ReadFileAsync(path, ct) ?? string.Empty;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) {
            Logger.Debug("Markdown fetch canceled for article {Id} ({Locale}).", article.Id, locale);
            return string.Empty;
        }
        catch (TaskCanceledException) {
            Logger.Warning("Markdown fetch timed out for article {Id} ({Locale}).", article.Id, locale);
            return string.Empty;
        }
        catch (Exception ex) {
            Logger.Warning(ex, "Failed to fetch markdown for article {Id} ({Locale}).", article.Id, locale);
            return string.Empty;
        }
    }

    public override StoryContent Create(Guid id = default, string? internalTitle = null) {
        if (id == Guid.Empty) id = Guid.CreateVersion7();
        DateTime now = DateTime.UtcNow;
        IReadOnlyCollection<LocalizationInfo> locals = localizationProvider.GetSupportedLocalizations();

        Dictionary<string, string> titles = locals.ToDictionary(c => c.Code, _ => "New Story");
        Dictionary<string, string> summaries = locals.ToDictionary(c => c.Code, c => $"{c.DisplayName} - Summary here");
        var article = new StoryContent {
            Id = id,
            Title = titles,
            Summary = summaries,
            Tags = [
            ],
            CreatedAt = now,
            LastModifiedAt = now,
            InternalTitle = internalTitle ?? string.Empty
        };
        Logger.Information("Created new article stub {Id}.", article.Id);
        return article;
    }

    public async Task<(Dictionary<string, string> Stubs, bool WroteAll)> GenerateStubsAsync(StoryContent article, bool writeToDisk = false, CancellationToken ct = default) {
        IReadOnlyCollection<LocalizationInfo> locals = localizationProvider.GetSupportedLocalizations();
        Dictionary<string, string> stubs = locals.ToDictionary(
            c => c.Code,
            c => $"# {article.Title.GetValueOrDefault(c.Code)}");

        if (!writeToDisk) {
            Logger.Debug("Generated article stubs for {Id} without writing to disk.", article.Id);
            return (stubs, false);
        }

        bool wroteAll = true;
        foreach (KeyValuePair<string, string> stub in stubs) {
            string path = Storage.GetMarkdownDiskPath(stub.Key, article.MarkdownFileName);
            if (!await Storage.WriteFileAsync(path, stub.Value, ct)) {
                wroteAll = false;
            }
        }

        Logger.Information("Generated and wrote article stubs for {Id} {Result}.", article.Id, wroteAll ? "succeeded" : "failed");
        return (stubs, wroteAll);
    }

    public async Task EnsureResxAsync(CancellationToken ct = default) {
        _ = ct;
        Logger.Debug("Resx generation is disabled in R2-only mode.");
        await Task.CompletedTask;
    }
}
