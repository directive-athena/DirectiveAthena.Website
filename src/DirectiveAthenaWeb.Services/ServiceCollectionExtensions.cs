// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Services.Contact;
using DirectiveAthenaWeb.Services.Localization;
using DirectiveAthenaWeb.Services.Markdown;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;

namespace DirectiveAthenaWeb.Services;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public static class ServiceCollectionExtensions {
    extension(IServiceCollection services) {
        [UsedImplicitly] public IServiceCollection AddWebsiteServices() {
            services.AddInfiniBlazor(static config => {
                config.Components.SetRenderMode(RenderMode.InteractiveWebAssembly);
                config.Markdown.WithMudBlazorComponents();
            });

            services.Configure<ContactInfoOptions>(options => {
                options
                    .AddContactInfo("BlueSky", "https://bsky.app/profile/annasas.dev", "https://simpleicons.org/icons/bluesky.svg")
                    .AddContactInfo("Twitter / X", "https://x.com/AnnaSasDev", "https://simpleicons.org/icons/x.svg")
                    .AddContactInfo("GitHub - Website", "https://github.com/directive-athena/Website", "https://simpleicons.org/icons/github.svg")
                    .AddContactInfo("GitHub - AnnaSasDev", "https://github.com/AnnaSasDev", "https://simpleicons.org/icons/github.svg", false)
                    .AddContactInfo("Twitch", "https://twitch.tv/AnnaSasDev", "https://simpleicons.org/icons/twitch.svg")
                    .AddContactInfo("YouTube", "https://twitch.tv/AnnaSasDev", "https://simpleicons.org/icons/youtube.svg");
            });
            
            services.Configure<LocalizationOptions>(options => {
                options
                    .AddLocalization("en", "English", "EN", "https://flagcdn.com/w40/us.png")
                    .AddLocalization("nl", "Nederlands", "NL", "https://flagcdn.com/w40/nl.png");
            });

            services.RegisterServicesFromDirectiveAthenaWebServices();
            services.RegisterServicesFromDirectiveAthenaWebServicesContact();
            services.RegisterServicesFromDirectiveAthenaWebServicesContent();
            services.RegisterServicesFromDirectiveAthenaWebServicesJs();
            services.RegisterServicesFromDirectiveAthenaWebServicesLocalization();

            return services;
        }
    }

}
