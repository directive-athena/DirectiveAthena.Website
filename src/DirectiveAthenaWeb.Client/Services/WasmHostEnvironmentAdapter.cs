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
internal sealed class WasmHostEnvironmentAdapter(IWebAssemblyHostEnvironment wasmEnvironment) : IHostEnvironment {
    public string EnvironmentName { get; set; } = wasmEnvironment.Environment;
    public string ApplicationName { get; set; } = "DirectiveAthenaWeb";
    public string ContentRootPath { get; set; } = wasmEnvironment.BaseAddress;
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
}
