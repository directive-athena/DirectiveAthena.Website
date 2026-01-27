// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Services;
using DirectiveAthenaWeb.Services.Contact;
using DirectiveAthenaWeb.Services.ContentStorage;
using MudBlazor;
using MudBlazor.Services;

namespace DirectiveAthenaWeb.DevServer;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public static class Program {
    public static void Main(string[] args) {
        // -------------------------------------------------------------------------------------------------------------
        // Builder
        // -------------------------------------------------------------------------------------------------------------
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.Services.Configure<R2StorageOptions>(builder.Configuration.GetSection("ContentStorage:R2"));
        builder.Services.Configure<ContactInfoOptions>(options => {
            options.Items = [
                new ContactInfoItem { Title = "BlueSky", Url = "https://bsky.app/profile/annasas.dev", SimpleIconsUrl = CustomIcons.Bluesky },
                new ContactInfoItem { Title = "Twitter / X", Url = "https://x.com/AnnaSasDev", SimpleIconsUrl = Icons.Custom.Brands.X },
                new ContactInfoItem { Title = "GitHub - Website", Url = "https://github.com/directive-athena/Website", SimpleIconsUrl = Icons.Custom.Brands.GitHub },
                new ContactInfoItem { Title = "GitHub - AnnaSasDev", Url = "https://github.com/AnnaSasDev", SimpleIconsUrl = Icons.Custom.Brands.GitHub, IncludeInFooter = false },
                new ContactInfoItem { Title = "Twitch", Url = "https://twitch.tv/AnnaSasDev", SimpleIconsUrl = CustomIcons.Twitch },
                new ContactInfoItem { Title = "YouTube", Url = "https://twitch.tv/AnnaSasDev", SimpleIconsUrl = Icons.Custom.Brands.YouTube }
            ];
        });
        builder.Services.AddMudServices();
        builder.Services.AddLocalization();
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();
        builder.Services.AddHttpClient();
        builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient());
        
        builder.Services.AddCors(options => {
            options.AddDefaultPolicy(policy => policy
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod());
        });
        
        builder.Services.AddWebsiteServices();
        
        builder.Services.AddWritingContent();
        builder.Services.AddFaqContent();

        // -------------------------------------------------------------------------------------------------------------
        // App
        // -------------------------------------------------------------------------------------------------------------
        WebApplication app = builder.Build();

        app.UseCors();
        app.UseBlazorFrameworkFiles();
        app.UseStaticFiles();
        app.UseAntiforgery();

        app.MapPost("/upload", ApiEndpoints.HandleUpload);
        app.MapPost("/delete", ApiEndpoints.HandleDelete);

        app.MapRazorComponents<AdminApp>()
            .AddInteractiveServerRenderMode();

        app.MapFallbackToFile("index.html");
        app.Run();
    }
}
