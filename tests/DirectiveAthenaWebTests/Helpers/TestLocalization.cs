// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Services.Localization;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace DirectiveAthenaWebTests.Helpers;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public static class TestLocalization {
    public static IReadOnlyCollection<LocalizationInfo> DefaultLocalizations()
        => [
            new("en", "English", "EN", "https://flagcdn.com/w40/us.png"),
            new("nl", "Nederlands", "NL", "https://flagcdn.com/w40/nl.png")
        ];

    public static IReadOnlyCollection<LocalizationInfo> BuildLocalizations(params string[] codes)
        => codes
            .Select(code => new LocalizationInfo(code, code.ToUpperInvariant(), code.ToUpperInvariant(), $"flags/{code}.png"))
            .ToArray();

    public static ILocalizationProvider CreateLocalizationProvider(
        string currentCode = "en",
        IReadOnlyCollection<LocalizationInfo>? localizations = null
    ) {
        LocalizationInfo[] locals = (localizations ?? DefaultLocalizations()).ToArray();
        LocalizationInfo current = locals.FirstOrDefault(l => l.Code == currentCode) ?? locals.First();

        var provider = Substitute.For<ILocalizationProvider>();
        provider.GetSupportedLocalizations().Returns(locals);
        provider.DefaultLocalization.Returns(locals.First());
        provider.GetCurrentLocalization().Returns(current);
        return provider;
    }

    public static ILocalizationProvider CreateLocalizationProviderForCodes(params string[] codes)
        => CreateLocalizationProvider(codes.FirstOrDefault() ?? "en", BuildLocalizations(codes));

    public static IOptions<LocalizationOptions> CreateLocalizationOptions(
        IReadOnlyCollection<LocalizationInfo>? localizations = null
    ) {
        LocalizationInfo[] locals = (localizations ?? DefaultLocalizations()).ToArray();
        var options = new LocalizationOptions();
        foreach (LocalizationInfo localization in locals) {
            options.AddLocalization(localization.Code, localization.DisplayName, localization.Abbreviation, localization.FlagPath);
        }

        return Options.Create(options);
    }
}
