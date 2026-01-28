// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Services;
using DirectiveAthenaWeb.Services.Contact;
using DirectiveAthenaWeb.Services.ContentStorage;
using DirectiveAthenaWeb.Services.Localization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using System.Net.Http;

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
