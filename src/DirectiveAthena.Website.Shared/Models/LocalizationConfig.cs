namespace DirectiveAthena.Website.Models;

public static class LocalizationConfig {
    public static readonly List<CultureConfig> SupportedCultures = [
        new("en", "English", "EN", "https://flagcdn.com/w40/us.png"),
        new("nl", "Nederlands", "NL", "https://flagcdn.com/w40/nl.png")
    ];

    public static CultureConfig DefaultCulture => SupportedCultures.First();
}