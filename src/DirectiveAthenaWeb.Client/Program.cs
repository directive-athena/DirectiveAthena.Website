// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Client.Services;
using DirectiveAthenaWeb.Content;
using DirectiveAthenaWeb.Services;
using DirectiveAthenaWeb.Services.Contact;
using DirectiveAthenaWeb.Services.ContentStorage;
using DirectiveAthenaWeb.Services.Localization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Hosting;
using MudBlazor.Services;

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

        builder.AddWebsiteLogging();

        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
        builder.Services.AddSingleton<IHostEnvironment>(_ => new WasmHostEnvironmentAdapter(builder.HostEnvironment));
        builder.Services.AddSingleton<IR2StatusTracker, R2StatusTracker>();
        builder.Services.AddMudServices();
        builder.Services.AddLocalization();

        builder.Services.AddWebsiteServices();
        builder.Services.AddWebsiteContent();
        
        builder.Services.Configure<R2StorageOptions>(builder.Configuration.GetSection("ContentStorage:R2"));
        builder.Services.Configure<ContactInfoOptions>(builder.Configuration.GetSection("ContactInfo"));
        builder.Services.Configure<LocalizationOptions>(builder.Configuration.GetSection("Localization"));
        
        builder.Services.AddNoteContent();
        builder.Services.AddFaqContent();
        builder.Services.AddStoryContent();

        // -------------------------------------------------------------------------------------------------------------
        // App
        // -------------------------------------------------------------------------------------------------------------
        WebAssemblyHost host = builder.Build();

        var cultureInitializer = host.Services.GetRequiredService<ILocalizationInitializer>();
        await cultureInitializer.ApplyPreferredCultureAsync();

        await host.RunAsync();
    }
}
