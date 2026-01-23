// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using MudBlazor;

namespace DirectiveAthena.Website.Services.Contact;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableSingleton<IContactInfoProvider>]
public class ContactInfoProvider : IContactInfoProvider {
    private readonly ContactInfo[] ContactInfos = [
        new("BlueSky", "https://bsky.app/profile/annasas.dev", CustomIcons.Bluesky),
        new("Twitter / X", "https://x.com/AnnaSasDev", Icons.Custom.Brands.X),
        new("GitHub - Website", "https://github.com/directive-athena/Website", Icons.Custom.Brands.GitHub),
        new("GitHub - AnnaSasDev", "https://github.com/AnnaSasDev", Icons.Custom.Brands.GitHub, IncludeInFooter:false),
        new("Twitch", "https://twitch.tv/AnnaSasDev", CustomIcons.Twitch),
        new("YouTube", "https://twitch.tv/AnnaSasDev", Icons.Custom.Brands.YouTube)
    ];

    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    public ContactInfo[] GetContactInfos() => ContactInfos;
}
