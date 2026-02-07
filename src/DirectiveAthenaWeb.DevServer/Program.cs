// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Content;
using DirectiveAthenaWeb.Services;
using DirectiveAthenaWeb.Services.Contact;
using DirectiveAthenaWeb.Services.R2Storage;
using DirectiveAthenaWeb.Services.Localization;
using MudBlazor.Services;
using System.Reflection;

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

        builder.AddWebsiteLogging();
        
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
        builder.Services.AddWebsiteContent();
        
        builder.Services.Configure<R2StorageOptions>(builder.Configuration.GetSection("ContentStorage:R2"));
        builder.Services.Configure<ContactInfoOptions>(builder.Configuration.GetSection("ContactInfo"));
        builder.Services.Configure<LocalizationOptions>(builder.Configuration.GetSection("Localization"));
        
        builder.Services.AddNoteContent(out Assembly noteAssembly);
        builder.Services.AddFaqContent(out Assembly faqAssembly);
        builder.Services.AddStoryContent(out Assembly storyAssembly);

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
        
        app.MapGet("/_config/client", (IConfiguration config) => {
            // Env vars are already part of IConfiguration by default.
            bool includeDevContent = config.GetValue<bool>("INFINILORE_INCLUDE_DEVCONTENT");
            return Results.Ok(new { includeDevContent });
        });
        
        app.MapRazorComponents<AdminApp>()
            .AddInteractiveServerRenderMode()
            .AddAdditionalAssemblies(noteAssembly, faqAssembly, storyAssembly);

        app.MapFallbackToFile("index.html");
        app.Run();
    }
}
