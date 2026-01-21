using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using DirectiveAthena.Website.Services;
using System.Globalization;
using Microsoft.JSInterop;
using DirectiveAthena.Website.Models;

namespace DirectiveAthena.Website;

public static class Program {
    public static async Task Main(string[] args) {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
        builder.Services.AddMudServices();
        builder.Services.AddScoped<WritingsService>();
        builder.Services.AddScoped<DevFileSystemService>();
        builder.Services.AddLocalization();

        WebAssemblyHost host = builder.Build();

        var jsInterop = host.Services.GetRequiredService<IJSRuntime>();
        var result = await jsInterop.InvokeAsync<string>("localStorage.getItem", "culture");
        
        string culture = LocalizationConfig.SupportedCultures.Any(c => c.Code == result) 
            ? result! 
            : LocalizationConfig.DefaultCulture.Code;
        
        var cultureInfo = new CultureInfo(culture);
        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

        await host.RunAsync();
    }
}