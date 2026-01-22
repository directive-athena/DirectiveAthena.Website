// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using DirectiveAthena.Website.Services;
using DirectiveAthena.Website.Services.InfiniMudMarkdown;

namespace DirectiveAthena.Website;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public static class Program {
    public static async Task Main(string[] args) {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        builder.Services.AddInfiniBlazor(static config => {
            config.Components.SetRenderMode(RenderMode.InteractiveWebAssembly);
            config.Markdown.WithMudBlazorComponents();
        });

        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
        builder.Services.AddMudServices();
        builder.Services.AddLocalization();

        builder.Services.RegisterServicesFromDirectiveAthenaWebsite();

        WebAssemblyHost host = builder.Build();

        var cultureInitializer = host.Services.GetRequiredService<ILocalizationInitializer>();
        await cultureInitializer.ApplyPreferredCultureAsync();
        
        await host.RunAsync();
    }
}
