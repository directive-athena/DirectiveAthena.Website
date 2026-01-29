// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace DirectiveAthenaWeb.Client.Services;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
internal sealed class WasmHostEnvironmentAdapter : IHostEnvironment {
    public WasmHostEnvironmentAdapter(IWebAssemblyHostEnvironment wasmEnvironment) {
        EnvironmentName = wasmEnvironment.Environment;
        ApplicationName = "DirectiveAthenaWeb";
        ContentRootPath = wasmEnvironment.BaseAddress;
        ContentRootFileProvider = new NullFileProvider();
    }

    public string EnvironmentName { get; set; }
    public string ApplicationName { get; set; }
    public string ContentRootPath { get; set; }
    public IFileProvider ContentRootFileProvider { get; set; }
}
