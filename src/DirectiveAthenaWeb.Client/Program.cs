// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using DirectiveAthenaWeb.Services;
using DirectiveAthenaWeb.Services.ContentStorage;
using DirectiveAthenaWeb.Services.Localization;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace DirectiveAthenaWeb.Client;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public static class Program {
    public static async Task Main(string[] args) {
        // -------------------------------------------------------------------------------------------------------------
        // Builder
        // -------------------------------------------------------------------------------------------------------------
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: false);

        LogEventLevel minimumLevel = builder.HostEnvironment.IsDevelopment()
            ? LogEventLevel.Debug
            : LogEventLevel.Information;
        Logger logger = new LoggerConfiguration()
            .MinimumLevel.Is(minimumLevel)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "DirectiveAthena.Website")
            .WriteTo.BrowserConsole()
            .WriteTo.Console()
            .CreateLogger();
        Log.Logger = logger;

        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(logger, dispose: true);

        #if DEBUG
        await TryLoadLocalSettingsAsync(builder, logger);
        #endif
        
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
        builder.Services.AddMudServices();
        builder.Services.AddLocalization();
        builder.Services.Configure<R2StorageOptions>(builder.Configuration.GetSection("ContentStorage:R2"));

        builder.Services.AddWebsiteServices();

        // -------------------------------------------------------------------------------------------------------------
        // App
        // -------------------------------------------------------------------------------------------------------------
        WebAssemblyHost host = builder.Build();

        var cultureInitializer = host.Services.GetRequiredService<ILocalizationInitializer>();
        await cultureInitializer.ApplyPreferredCultureAsync();

        await host.RunAsync();
    }

    #if DEBUG
    private static async Task TryLoadLocalSettingsAsync(WebAssemblyHostBuilder builder, Logger logger) {
        try {
            using HttpClient http = new();
            http.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
            await using Stream stream = await http.GetStreamAsync("appsettings.local.json");
            using MemoryStream buffer = new();
            await stream.CopyToAsync(buffer);
            buffer.Position = 0;
            builder.Configuration.AddJsonStream(buffer);
            logger.Information("Loaded appsettings.local.json via HTTP.");
        }
        catch (Exception ex) {
            logger.Debug(ex, "appsettings.local.json not loaded.");
        }
    }
    #endif
}
