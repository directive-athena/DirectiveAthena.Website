// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Services;
using DirectiveAthenaWeb.Services.Contact;
using DirectiveAthenaWeb.Services.ContentStorage;
using DirectiveAthenaWeb.Services.Localization;
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
        builder.Services.AddMudServices();
        builder.Services.AddLocalization();
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();
        builder.Services.AddHttpClient();
        builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient());
        builder.Services.AddSingleton<IR2StatusTracker, R2StatusTracker>();
        
        builder.Services.AddCors(options => {
            options.AddDefaultPolicy(policy => policy
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod());
        });
        
        builder.Services.AddWebsiteServices();

        builder.Services.Configure<R2StorageOptions>(builder.Configuration.GetSection("ContentStorage:R2"));
        builder.Services.Configure<ContactInfoOptions>(builder.Configuration.GetSection("ContactInfo"));
        builder.Services.Configure<LocalizationOptions>(builder.Configuration.GetSection("Localization"));

        
        builder.Services.AddNoteContent();
        builder.Services.AddFaqContent();
        builder.Services.AddStoryContent();

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
