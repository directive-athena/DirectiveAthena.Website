// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Client.Services;
using DirectiveAthenaWeb.Services;
using DirectiveAthenaWeb.Services.Contact;
using DirectiveAthenaWeb.Services.ContentStorage;
using DirectiveAthenaWeb.Services.Localization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Hosting;
using MudBlazor.Services;
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

        LogEventLevel minimumLevel = builder.HostEnvironment.IsDevelopment()
            ? LogEventLevel.Debug
            : LogEventLevel.Information;
        Logger logger = new LoggerConfiguration()
            .MinimumLevel.Is(minimumLevel)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "DirectiveAthenaWeb")
            .WriteTo.BrowserConsole()
            .WriteTo.Console()
            .CreateLogger();
        Log.Logger = logger;

        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(logger, dispose: true);

        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
        builder.Services.AddSingleton<IHostEnvironment>(_ => new WasmHostEnvironmentAdapter(builder.HostEnvironment));
        builder.Services.AddSingleton<IR2StatusTracker, R2StatusTracker>();
        builder.Services.AddMudServices();
        builder.Services.AddLocalization();

        builder.Services.AddWebsiteServices();
        builder.Services.Configure<R2StorageOptions>(builder.Configuration.GetSection("ContentStorage:R2"));
        builder.Services.Configure<ContactInfoOptions>(builder.Configuration.GetSection("ContactInfo"));
        builder.Services.Configure<LocalizationOptions>(builder.Configuration.GetSection("Localization"));
        
        builder.Services.AddNoteContent();
        builder.Services.AddFaqContent();

        // -------------------------------------------------------------------------------------------------------------
        // App
        // -------------------------------------------------------------------------------------------------------------
        WebAssemblyHost host = builder.Build();

        var cultureInitializer = host.Services.GetRequiredService<ILocalizationInitializer>();
        await cultureInitializer.ApplyPreferredCultureAsync();

        await host.RunAsync();
    }
}
