// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using DirectiveAthena.Website.Services;
using DirectiveAthena.Website.Services.FileSystem;
using DirectiveAthena.Website.Services.Localization;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace DirectiveAthena.Website;
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
}
