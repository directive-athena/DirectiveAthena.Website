// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Services;
using DirectiveAthenaWeb.Services.ContentStorage;
using JetBrains.Annotations;
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
        builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: false);
        builder.Services.Configure<R2StorageOptions>(builder.Configuration.GetSection("ContentStorage:R2"));
        builder.Services.AddMudServices();
        builder.Services.AddLocalization();
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();
        builder.Services.AddHttpClient();
        builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient());
        builder.Services.AddWebsiteServices();
        builder.Services.AddCors(options => {
            options.AddDefaultPolicy(policy => policy
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod());
        });

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

[UsedImplicitly] internal record ProxyUploadRequest(string Key, string Content, string ContentType);
[UsedImplicitly] internal record ProxyDeleteRequest(string Key);
