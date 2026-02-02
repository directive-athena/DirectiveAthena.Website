// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using JetBrains.Annotations;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace DirectiveAthenaWeb.Services;

// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public static class LoggingExtensions {
    private static LoggerConfiguration ConfigureLogger() {
        return new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "DirectiveAthenaWeb")
            .WriteTo.Console(theme:AnsiConsoleTheme.Literate, applyThemeToRedirectedOutput: true)
            .WriteTo.BrowserConsole();
    }

    extension(WebAssemblyHostBuilder builder) {
        [UsedImplicitly] public WebAssemblyHostBuilder AddWebsiteLogging() {
            LogEventLevel minimumLevel = builder.HostEnvironment.IsDevelopment()
                ? LogEventLevel.Debug
                : LogEventLevel.Information;

            LoggerConfiguration loggerConfig = ConfigureLogger()
                .MinimumLevel.Is(minimumLevel)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning);
            
            Logger logger = loggerConfig.CreateLogger();
            Log.Logger = logger;
            
            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(logger, dispose: true);
            
            return builder;
        }
    }
    
    extension(IHostApplicationBuilder builder) {
        [UsedImplicitly] public IHostApplicationBuilder AddWebsiteLogging() {
            
            // LogEventLevel minimumLevel = builder.Environment.IsDevelopment()
            //     ? LogEventLevel.Debug
            //     : LogEventLevel.Information;

            LoggerConfiguration loggerConfig = ConfigureLogger()
                .MinimumLevel.Is(LogEventLevel.Information);
            
            Logger logger = loggerConfig.CreateLogger();
            Log.Logger = logger;
            
            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(logger, dispose: true);

            return builder;
        }
    }
    
}
