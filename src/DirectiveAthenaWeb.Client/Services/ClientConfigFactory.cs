// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthenaWeb.Services.Client;
using System.Net.Http.Json;

namespace DirectiveAthenaWeb.Client.Services;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableScoped<IClientConfigFactory>]
public class ClientConfigFactory(IHttpClientFactory clientFactory, ILogger<ClientConfigFactory> logger) : IClientConfigFactory{
    private ClientConfig? _config;
    
    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    public async ValueTask<ClientConfig> CreateAsync() {
        if (_config is not null) return _config;
        
        #if DEBUG
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        CancellationToken token = cts.Token;

        try {
            HttpClient client = clientFactory.CreateClient("Server");
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
        #else
        // In release mode there is no server to contact
        return new ClientConfig();
        #endif
    }
}
