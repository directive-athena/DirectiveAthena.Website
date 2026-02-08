// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthenaWeb.Services.Client;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
#if DEBUG
using System.Net.Http.Json;
#endif

namespace DirectiveAthenaWeb.Services;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableScoped<IClientConfigFactory>]
public class ClientConfigFactory(
    IHttpClientFactory clientFactory,
    ILogger<ClientConfigFactory> logger,
    NavigationManager? navigationManager = null) : IClientConfigFactory {
    private ClientConfig? _config;

    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    #if DEBUG
    public async ValueTask<ClientConfig> CreateAsync(CancellationToken ct = default) {
        if (_config is not null) return _config;

        CancellationToken token;
        if (ct == CancellationToken.None) {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            token = cts.Token;
        }
        else token = ct;

        try {
            HttpClient client = clientFactory.CreateClient("Server");
            if (client.BaseAddress is null && navigationManager is not null) {
                client.BaseAddress = new Uri(navigationManager.BaseUri, UriKind.Absolute);
            }
            HttpResponseMessage response = await client.GetAsync("/_config/client", token);
            if (!response.IsSuccessStatusCode) {
                logger.Warning("Failed to fetch client config: {StatusCode}", response.StatusCode);
                return _config = new ClientConfig();
            }

            var config = await response.Content.ReadFromJsonAsync<ClientConfig>(cancellationToken: token);
            return _config = config ?? new ClientConfig();
        }
        catch (Exception e) {
            logger.Error(e, "Failed to fetch client config");
            return _config = new ClientConfig();
        }
    }
    #else
    public ValueTask<ClientConfig> CreateAsync(CancellationToken ct = default) {
        _ = clientFactory;
        _ = logger;
        return ValueTask.FromResult(_config ??= new ClientConfig());
    }
    #endif
}
